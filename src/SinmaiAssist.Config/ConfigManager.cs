using System;
using System.IO;
using MelonLoader;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SinmaiAssist.Config;

public class ConfigManager<T> where T : new()
{
	private readonly string _configPath;

	private T _config;

	private readonly IYamlTypeConverter _customConverter;

	internal ConfigManager(string configPath = "config.yml", IYamlTypeConverter customConverter = null)
	{
		_configPath = configPath;
		_customConverter = customConverter;
		InitConfig();
	}

	private void InitConfig()
	{
		try
		{
			if (!File.Exists(_configPath))
			{
				_config = new T();
				SaveConfig();
				MelonLogger.Msg("Create Default Config '" + _configPath + "' ");
			}
			else
			{
				LoadConfig();
			}
		}
		catch (YamlException ex)
		{
			MelonLogger.Error("Load Config '" + _configPath + "' Failed: \n" + ex.Message);
			MelonLogger.Warning("Your Config is not valid, please delete it and restart the game.");
			throw;
		}
		catch (Exception ex2)
		{
			MelonLogger.Error("Init Config '" + _configPath + "' Failed: \n" + ex2.Message);
			throw;
		}
	}

	public T GetConfig()
	{
		if (_config == null)
		{
			throw new InvalidOperationException("Configuration is not initialized.");
		}
		return _config;
	}

	public void Save()
	{
		SaveConfig();
	}

	private ISerializer CreateSerializer()
	{
		SerializerBuilder serializerBuilder = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance);
		if (_customConverter != null)
		{
			serializerBuilder = serializerBuilder.WithTypeConverter(_customConverter);
		}
		return serializerBuilder.Build();
	}

	private IDeserializer CreateDeserializer()
	{
		DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.IgnoreUnmatchedProperties();
		if (_customConverter != null)
		{
			deserializerBuilder = deserializerBuilder.WithTypeConverter(_customConverter);
		}
		return deserializerBuilder.Build();
	}

	private void SaveConfig()
	{
		string directoryName = Path.GetDirectoryName(_configPath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		string contents = CreateSerializer().Serialize(_config);
		File.WriteAllText(_configPath, contents);
	}

	private void LoadConfig()
	{
		IDeserializer deserializer = CreateDeserializer();
		string input = File.ReadAllText(_configPath);
		_config = deserializer.Deserialize<T>(input);
	}
}
