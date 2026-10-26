using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal class Parser : IParser
{
	private class EventQueue
	{
		private readonly Queue<ParsingEvent> highPriorityEvents = new Queue<ParsingEvent>();

		private readonly Queue<ParsingEvent> normalPriorityEvents = new Queue<ParsingEvent>();

		public int Count => highPriorityEvents.Count + normalPriorityEvents.Count;

		public void Enqueue(ParsingEvent @event)
		{
			EventType type = @event.Type;
			if (type == EventType.StreamStart || type == EventType.DocumentStart)
			{
				highPriorityEvents.Enqueue(@event);
			}
			else
			{
				normalPriorityEvents.Enqueue(@event);
			}
		}

		public ParsingEvent Dequeue()
		{
			if (highPriorityEvents.Count <= 0)
			{
				return normalPriorityEvents.Dequeue();
			}
			return highPriorityEvents.Dequeue();
		}
	}

	private readonly Stack<ParserState> states = new Stack<ParserState>();

	private readonly TagDirectiveCollection tagDirectives = new TagDirectiveCollection();

	private ParserState state;

	private readonly IScanner scanner;

	private Token? currentToken;

	private VersionDirective? version;

	private readonly EventQueue pendingEvents = new EventQueue();

	public ParsingEvent? Current { get; private set; }

	private Token? GetCurrentToken()
	{
		if (currentToken == null)
		{
			while (scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (!(currentToken is YamlDotNet.Core.Tokens.Comment comment))
				{
					break;
				}
				pendingEvents.Enqueue(new YamlDotNet.Core.Events.Comment(comment.Value, comment.IsInline, comment.Start, comment.End));
				scanner.ConsumeCurrent();
			}
		}
		return currentToken;
	}

	public Parser(TextReader input)
		: this(new Scanner(input))
	{
	}

	public Parser(IScanner scanner)
	{
		this.scanner = scanner;
	}

	public bool MoveNext()
	{
		if (state == ParserState.StreamEnd)
		{
			Current = null;
			return false;
		}
		if (pendingEvents.Count == 0)
		{
			pendingEvents.Enqueue(StateMachine());
		}
		Current = pendingEvents.Dequeue();
		return true;
	}

	private ParsingEvent StateMachine()
	{
		return state switch
		{
			ParserState.StreamStart => ParseStreamStart(), 
			ParserState.ImplicitDocumentStart => ParseDocumentStart(isImplicit: true), 
			ParserState.DocumentStart => ParseDocumentStart(isImplicit: false), 
			ParserState.DocumentContent => ParseDocumentContent(), 
			ParserState.DocumentEnd => ParseDocumentEnd(), 
			ParserState.BlockNode => ParseNode(isBlock: true, isIndentlessSequence: false), 
			ParserState.BlockNodeOrIndentlessSequence => ParseNode(isBlock: true, isIndentlessSequence: true), 
			ParserState.FlowNode => ParseNode(isBlock: false, isIndentlessSequence: false), 
			ParserState.BlockSequenceFirstEntry => ParseBlockSequenceEntry(isFirst: true), 
			ParserState.BlockSequenceEntry => ParseBlockSequenceEntry(isFirst: false), 
			ParserState.IndentlessSequenceEntry => ParseIndentlessSequenceEntry(), 
			ParserState.BlockMappingFirstKey => ParseBlockMappingKey(isFirst: true), 
			ParserState.BlockMappingKey => ParseBlockMappingKey(isFirst: false), 
			ParserState.BlockMappingValue => ParseBlockMappingValue(), 
			ParserState.FlowSequenceFirstEntry => ParseFlowSequenceEntry(isFirst: true), 
			ParserState.FlowSequenceEntry => ParseFlowSequenceEntry(isFirst: false), 
			ParserState.FlowSequenceEntryMappingKey => ParseFlowSequenceEntryMappingKey(), 
			ParserState.FlowSequenceEntryMappingValue => ParseFlowSequenceEntryMappingValue(), 
			ParserState.FlowSequenceEntryMappingEnd => ParseFlowSequenceEntryMappingEnd(), 
			ParserState.FlowMappingFirstKey => ParseFlowMappingKey(isFirst: true), 
			ParserState.FlowMappingKey => ParseFlowMappingKey(isFirst: false), 
			ParserState.FlowMappingValue => ParseFlowMappingValue(isEmpty: false), 
			ParserState.FlowMappingEmptyValue => ParseFlowMappingValue(isEmpty: true), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private void Skip()
	{
		if (currentToken != null)
		{
			currentToken = null;
			scanner.ConsumeCurrent();
		}
	}

	private ParsingEvent ParseStreamStart()
	{
		Token token = GetCurrentToken();
		if (!(token is YamlDotNet.Core.Tokens.StreamStart streamStart))
		{
			throw new SemanticErrorException(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty, "Did not find expected <stream-start>.");
		}
		Skip();
		state = ParserState.ImplicitDocumentStart;
		return new YamlDotNet.Core.Events.StreamStart(streamStart.Start, streamStart.End);
	}

	private ParsingEvent ParseDocumentStart(bool isImplicit)
	{
		if (currentToken is VersionDirective)
		{
			throw new SyntaxErrorException("While parsing a document start node, could not find document end marker before version directive.");
		}
		Token token = GetCurrentToken();
		if (!isImplicit)
		{
			while (token is YamlDotNet.Core.Tokens.DocumentEnd)
			{
				Skip();
				token = GetCurrentToken();
			}
		}
		if (token == null)
		{
			throw new SyntaxErrorException("Reached the end of the stream while parsing a document start.");
		}
		if (token is YamlDotNet.Core.Tokens.Scalar && (state == ParserState.ImplicitDocumentStart || state == ParserState.DocumentStart))
		{
			isImplicit = true;
		}
		if ((isImplicit && !(token is VersionDirective) && !(token is TagDirective) && !(token is YamlDotNet.Core.Tokens.DocumentStart) && !(token is YamlDotNet.Core.Tokens.StreamEnd) && !(token is YamlDotNet.Core.Tokens.DocumentEnd)) || token is BlockMappingStart)
		{
			TagDirectiveCollection tags = new TagDirectiveCollection();
			ProcessDirectives(tags);
			states.Push(ParserState.DocumentEnd);
			state = ParserState.BlockNode;
			return new YamlDotNet.Core.Events.DocumentStart(null, tags, isImplicit: true, token.Start, token.End);
		}
		if (!(token is YamlDotNet.Core.Tokens.StreamEnd) && !(token is YamlDotNet.Core.Tokens.DocumentEnd))
		{
			Mark start = token.Start;
			TagDirectiveCollection tags2 = new TagDirectiveCollection();
			VersionDirective versionDirective = ProcessDirectives(tags2);
			token = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document start");
			if (!(token is YamlDotNet.Core.Tokens.DocumentStart))
			{
				throw new SemanticErrorException(token.Start, token.End, "Did not find expected <document start>.");
			}
			states.Push(ParserState.DocumentEnd);
			state = ParserState.DocumentContent;
			Mark end = token.End;
			Skip();
			return new YamlDotNet.Core.Events.DocumentStart(versionDirective, tags2, isImplicit: false, start, end);
		}
		if (token is YamlDotNet.Core.Tokens.DocumentEnd)
		{
			Skip();
		}
		state = ParserState.StreamEnd;
		token = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document start");
		YamlDotNet.Core.Events.StreamEnd result = new YamlDotNet.Core.Events.StreamEnd(token.Start, token.End);
		if (scanner.MoveNextWithoutConsuming())
		{
			throw new InvalidOperationException("The scanner should contain no more tokens.");
		}
		return result;
	}

	private VersionDirective? ProcessDirectives(TagDirectiveCollection tags)
	{
		bool flag = false;
		VersionDirective result = null;
		while (true)
		{
			if (GetCurrentToken() is VersionDirective versionDirective)
			{
				if (version != null)
				{
					throw new SemanticErrorException(versionDirective.Start, versionDirective.End, "Found duplicate %YAML directive.");
				}
				if (versionDirective.Version.Major != 1 || versionDirective.Version.Minor > 3)
				{
					throw new SemanticErrorException(versionDirective.Start, versionDirective.End, "Found incompatible YAML document.");
				}
				result = (version = versionDirective);
				flag = true;
			}
			else
			{
				if (!(GetCurrentToken() is TagDirective tagDirective))
				{
					break;
				}
				if (tags.Contains(tagDirective.Handle))
				{
					throw new SemanticErrorException(tagDirective.Start, tagDirective.End, "Found duplicate %TAG directive.");
				}
				tags.Add(tagDirective);
				flag = true;
			}
			Skip();
		}
		if (GetCurrentToken() is YamlDotNet.Core.Tokens.DocumentStart && (version == null || (version.Version.Major == 1 && version.Version.Minor > 1)))
		{
			if (GetCurrentToken() is YamlDotNet.Core.Tokens.DocumentStart && version == null)
			{
				version = new VersionDirective(new Version(1, 2));
			}
			flag = true;
		}
		AddTagDirectives(tags, Constants.DefaultTagDirectives);
		if (flag)
		{
			tagDirectives.Clear();
		}
		AddTagDirectives(tagDirectives, tags);
		return result;
	}

	private static void AddTagDirectives(TagDirectiveCollection directives, IEnumerable<TagDirective> source)
	{
		foreach (TagDirective item in source)
		{
			if (!directives.Contains(item))
			{
				directives.Add(item);
			}
		}
	}

	private ParsingEvent ParseDocumentContent()
	{
		if (GetCurrentToken() is VersionDirective || GetCurrentToken() is TagDirective || GetCurrentToken() is YamlDotNet.Core.Tokens.DocumentStart || GetCurrentToken() is YamlDotNet.Core.Tokens.DocumentEnd || GetCurrentToken() is YamlDotNet.Core.Tokens.StreamEnd)
		{
			state = states.Pop();
			return ProcessEmptyScalar(scanner.CurrentPosition);
		}
		return ParseNode(isBlock: true, isIndentlessSequence: false);
	}

	private static ParsingEvent ProcessEmptyScalar(in Mark position)
	{
		return new YamlDotNet.Core.Events.Scalar(AnchorName.Empty, TagName.Empty, string.Empty, ScalarStyle.Plain, isPlainImplicit: true, isQuotedImplicit: false, position, position);
	}

	private ParsingEvent ParseNode(bool isBlock, bool isIndentlessSequence)
	{
		if (GetCurrentToken() is Error { Start: var start } error)
		{
			throw new SemanticErrorException(in start, error.End, error.Value);
		}
		Token token = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
		if (token is YamlDotNet.Core.Tokens.AnchorAlias anchorAlias)
		{
			state = states.Pop();
			ParsingEvent result = new YamlDotNet.Core.Events.AnchorAlias(anchorAlias.Value, anchorAlias.Start, anchorAlias.End);
			Skip();
			return result;
		}
		Mark start2 = token.Start;
		AnchorName anchor = AnchorName.Empty;
		TagName tag = TagName.Empty;
		Anchor anchor2 = null;
		Tag tag2 = null;
		while (true)
		{
			if (anchor.IsEmpty && token is Anchor anchor3)
			{
				anchor2 = anchor3;
				anchor = anchor3.Value;
				Skip();
			}
			else
			{
				if (!tag.IsEmpty || !(token is Tag tag3))
				{
					if (token is Anchor { Start: var start3 } anchor4)
					{
						throw new SemanticErrorException(in start3, anchor4.End, "While parsing a node, found more than one anchor.");
					}
					if (token is YamlDotNet.Core.Tokens.AnchorAlias { Start: var start4 } anchorAlias2)
					{
						throw new SemanticErrorException(in start4, anchorAlias2.End, "While parsing a node, did not find expected token.");
					}
					if (!(token is Error error2))
					{
						break;
					}
					if (tag2 != null && anchor2 != null && !anchor.IsEmpty)
					{
						return new YamlDotNet.Core.Events.Scalar(anchor, default(TagName), string.Empty, ScalarStyle.Any, isPlainImplicit: false, isQuotedImplicit: false, anchor2.Start, anchor2.End);
					}
					throw new SemanticErrorException(error2.Start, error2.End, error2.Value);
				}
				tag2 = tag3;
				if (string.IsNullOrEmpty(tag3.Handle))
				{
					tag = new TagName(tag3.Suffix);
				}
				else
				{
					if (!tagDirectives.Contains(tag3.Handle))
					{
						throw new SemanticErrorException(tag3.Start, tag3.End, "While parsing a node, found undefined tag handle.");
					}
					tag = new TagName(tagDirectives[tag3.Handle].Prefix + tag3.Suffix);
				}
				Skip();
			}
			token = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
		}
		bool isEmpty = tag.IsEmpty;
		if (isIndentlessSequence && GetCurrentToken() is BlockEntry)
		{
			state = ParserState.IndentlessSequenceEntry;
			return new SequenceStart(anchor, tag, isEmpty, SequenceStyle.Block, start2, token.End);
		}
		if (token is YamlDotNet.Core.Tokens.Scalar scalar)
		{
			bool isPlainImplicit = false;
			bool isQuotedImplicit = false;
			if ((scalar.Style == ScalarStyle.Plain && tag.IsEmpty) || tag.IsNonSpecific)
			{
				isPlainImplicit = true;
			}
			else if (tag.IsEmpty)
			{
				isQuotedImplicit = true;
			}
			state = states.Pop();
			Skip();
			ParsingEvent result2 = new YamlDotNet.Core.Events.Scalar(anchor, tag, scalar.Value, scalar.Style, isPlainImplicit, isQuotedImplicit, start2, scalar.End, scalar.IsKey);
			if (!anchor.IsEmpty && scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (currentToken is Error)
				{
					Error error3 = currentToken as Error;
					throw new SemanticErrorException(error3.Start, error3.End, error3.Value);
				}
			}
			if (state == ParserState.FlowMappingKey && !(scanner.Current is FlowMappingEnd) && scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (currentToken != null && !(currentToken is FlowEntry) && !(currentToken is FlowMappingEnd))
				{
					throw new SemanticErrorException(currentToken.Start, currentToken.End, "While parsing a flow mapping, did not find expected ',' or '}'.");
				}
			}
			return result2;
		}
		if (token is FlowSequenceStart flowSequenceStart)
		{
			state = ParserState.FlowSequenceFirstEntry;
			return new SequenceStart(anchor, tag, isEmpty, SequenceStyle.Flow, start2, flowSequenceStart.End);
		}
		if (token is FlowMappingStart flowMappingStart)
		{
			state = ParserState.FlowMappingFirstKey;
			return new MappingStart(anchor, tag, isEmpty, MappingStyle.Flow, start2, flowMappingStart.End);
		}
		if (isBlock)
		{
			if (token is BlockSequenceStart blockSequenceStart)
			{
				state = ParserState.BlockSequenceFirstEntry;
				return new SequenceStart(anchor, tag, isEmpty, SequenceStyle.Block, start2, blockSequenceStart.End);
			}
			if (token is BlockMappingStart blockMappingStart)
			{
				state = ParserState.BlockMappingFirstKey;
				return new MappingStart(anchor, tag, isEmpty, MappingStyle.Block, start2, blockMappingStart.End);
			}
		}
		if (!anchor.IsEmpty || !tag.IsEmpty)
		{
			state = states.Pop();
			return new YamlDotNet.Core.Events.Scalar(anchor, tag, string.Empty, ScalarStyle.Plain, isEmpty, isQuotedImplicit: false, start2, token.End);
		}
		throw new SemanticErrorException(token.Start, token.End, "While parsing a node, did not find expected node content.");
	}

	private ParsingEvent ParseDocumentEnd()
	{
		Token token = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document end");
		bool isImplicit = true;
		Mark start = token.Start;
		Mark end = start;
		if (token is YamlDotNet.Core.Tokens.DocumentEnd)
		{
			end = token.End;
			Skip();
			isImplicit = false;
		}
		else if (!(currentToken is YamlDotNet.Core.Tokens.StreamEnd) && !(currentToken is YamlDotNet.Core.Tokens.DocumentStart) && !(currentToken is FlowSequenceEnd) && !(currentToken is VersionDirective) && (!(Current is YamlDotNet.Core.Events.Scalar) || !(currentToken is Error)))
		{
			throw new SemanticErrorException(in start, in end, "Did not find expected <document end>.");
		}
		if (version != null && version.Version.Major == 1 && version.Version.Minor > 1)
		{
			version = null;
		}
		state = ParserState.DocumentStart;
		return new YamlDotNet.Core.Events.DocumentEnd(isImplicit, start, end);
	}

	private ParsingEvent ParseBlockSequenceEntry(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		Token token = GetCurrentToken();
		if (token is BlockEntry { End: var position })
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is BlockEntry) && !(token is BlockEnd))
			{
				states.Push(ParserState.BlockSequenceEntry);
				return ParseNode(isBlock: true, isIndentlessSequence: false);
			}
			state = ParserState.BlockSequenceEntry;
			return ProcessEmptyScalar(in position);
		}
		if (token is BlockEnd blockEnd)
		{
			state = states.Pop();
			ParsingEvent result = new SequenceEnd(blockEnd.Start, blockEnd.End);
			Skip();
			return result;
		}
		throw new SemanticErrorException(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty, "While parsing a block collection, did not find expected '-' indicator.");
	}

	private ParsingEvent ParseIndentlessSequenceEntry()
	{
		Token token = GetCurrentToken();
		if (token is BlockEntry { End: var position })
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is BlockEntry) && !(token is Key) && !(token is Value) && !(token is BlockEnd))
			{
				states.Push(ParserState.IndentlessSequenceEntry);
				return ParseNode(isBlock: true, isIndentlessSequence: false);
			}
			state = ParserState.IndentlessSequenceEntry;
			return ProcessEmptyScalar(in position);
		}
		state = states.Pop();
		return new SequenceEnd(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty);
	}

	private ParsingEvent ParseBlockMappingKey(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		Token token = GetCurrentToken();
		if (token is Key { End: var position })
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is Key) && !(token is Value) && !(token is BlockEnd))
			{
				states.Push(ParserState.BlockMappingValue);
				return ParseNode(isBlock: true, isIndentlessSequence: true);
			}
			state = ParserState.BlockMappingValue;
			return ProcessEmptyScalar(in position);
		}
		if (token is Value value)
		{
			Skip();
			return ProcessEmptyScalar(value.End);
		}
		if (token is YamlDotNet.Core.Tokens.AnchorAlias anchorAlias)
		{
			Skip();
			return new YamlDotNet.Core.Events.AnchorAlias(anchorAlias.Value, anchorAlias.Start, anchorAlias.End);
		}
		if (token is BlockEnd blockEnd)
		{
			state = states.Pop();
			ParsingEvent result = new MappingEnd(blockEnd.Start, blockEnd.End);
			Skip();
			return result;
		}
		if (GetCurrentToken() is Error { Start: var start } error)
		{
			throw new SyntaxErrorException(in start, error.End, error.Value);
		}
		throw new SemanticErrorException(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty, "While parsing a block mapping, did not find expected key.");
	}

	private ParsingEvent ParseBlockMappingValue()
	{
		Token token = GetCurrentToken();
		if (token is Value { End: var position })
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is Key) && !(token is Value) && !(token is BlockEnd))
			{
				states.Push(ParserState.BlockMappingKey);
				return ParseNode(isBlock: true, isIndentlessSequence: true);
			}
			state = ParserState.BlockMappingKey;
			return ProcessEmptyScalar(in position);
		}
		if (token is Error { Start: var start } error)
		{
			throw new SemanticErrorException(in start, error.End, error.Value);
		}
		state = ParserState.BlockMappingKey;
		return ProcessEmptyScalar(token?.Start ?? Mark.Empty);
	}

	private ParsingEvent ParseFlowSequenceEntry(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		Token token = GetCurrentToken();
		ParsingEvent result;
		if (!(token is FlowSequenceEnd))
		{
			if (!isFirst)
			{
				if (!(token is FlowEntry))
				{
					throw new SemanticErrorException(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty, "While parsing a flow sequence, did not find expected ',' or ']'.");
				}
				Skip();
				token = GetCurrentToken();
			}
			if (token is Key)
			{
				state = ParserState.FlowSequenceEntryMappingKey;
				result = new MappingStart(AnchorName.Empty, TagName.Empty, isImplicit: true, MappingStyle.Flow);
				Skip();
				return result;
			}
			if (!(token is FlowSequenceEnd))
			{
				states.Push(ParserState.FlowSequenceEntry);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = states.Pop();
		result = new SequenceEnd(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty);
		Skip();
		return result;
	}

	private ParsingEvent ParseFlowSequenceEntryMappingKey()
	{
		Token token = GetCurrentToken();
		if (!(token is Value) && !(token is FlowEntry) && !(token is FlowSequenceEnd))
		{
			states.Push(ParserState.FlowSequenceEntryMappingValue);
			return ParseNode(isBlock: false, isIndentlessSequence: false);
		}
		Mark position = token?.End ?? Mark.Empty;
		Skip();
		state = ParserState.FlowSequenceEntryMappingValue;
		return ProcessEmptyScalar(in position);
	}

	private ParsingEvent ParseFlowSequenceEntryMappingValue()
	{
		Token token = GetCurrentToken();
		if (token is Value)
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is FlowEntry) && !(token is FlowSequenceEnd))
			{
				states.Push(ParserState.FlowSequenceEntryMappingEnd);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = ParserState.FlowSequenceEntryMappingEnd;
		return ProcessEmptyScalar(token?.Start ?? Mark.Empty);
	}

	private ParsingEvent ParseFlowSequenceEntryMappingEnd()
	{
		state = ParserState.FlowSequenceEntry;
		Token token = GetCurrentToken();
		return new MappingEnd(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty);
	}

	private ParsingEvent ParseFlowMappingKey(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		Token token = GetCurrentToken();
		if (!(token is FlowMappingEnd))
		{
			if (!isFirst)
			{
				if (token is FlowEntry)
				{
					Skip();
					token = GetCurrentToken();
				}
				else if (!(token is YamlDotNet.Core.Tokens.Scalar))
				{
					throw new SemanticErrorException(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty, "While parsing a flow mapping,  did not find expected ',' or '}'.");
				}
			}
			if (token is Key)
			{
				Skip();
				token = GetCurrentToken();
				if (!(token is Value) && !(token is FlowEntry) && !(token is FlowMappingEnd))
				{
					states.Push(ParserState.FlowMappingValue);
					return ParseNode(isBlock: false, isIndentlessSequence: false);
				}
				state = ParserState.FlowMappingValue;
				return ProcessEmptyScalar(token?.Start ?? Mark.Empty);
			}
			if (token is YamlDotNet.Core.Tokens.Scalar)
			{
				states.Push(ParserState.FlowMappingValue);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
			if (!(token is FlowMappingEnd))
			{
				states.Push(ParserState.FlowMappingEmptyValue);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = states.Pop();
		Skip();
		return new MappingEnd(token?.Start ?? Mark.Empty, token?.End ?? Mark.Empty);
	}

	private ParsingEvent ParseFlowMappingValue(bool isEmpty)
	{
		Token token = GetCurrentToken();
		if (!isEmpty && token is Value)
		{
			Skip();
			token = GetCurrentToken();
			if (!(token is FlowEntry) && !(token is FlowMappingEnd))
			{
				states.Push(ParserState.FlowMappingKey);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = ParserState.FlowMappingKey;
		if (!isEmpty && token is YamlDotNet.Core.Tokens.Scalar scalar)
		{
			Skip();
			return new YamlDotNet.Core.Events.Scalar(AnchorName.Empty, TagName.Empty, scalar.Value, scalar.Style, isPlainImplicit: false, isQuotedImplicit: false, token.Start, scalar.End);
		}
		return ProcessEmptyScalar(token?.Start ?? Mark.Empty);
	}
}
