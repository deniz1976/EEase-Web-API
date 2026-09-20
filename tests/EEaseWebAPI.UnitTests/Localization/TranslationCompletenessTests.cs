using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using EEaseWebAPI.API;
using EEaseWebAPI.Application;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Localization
{
    /// <summary>
    /// A missing translation does not fail anything at runtime: the localizer quietly falls
    /// back to English, and nobody notices until a Turkish user reads an English sentence.
    /// These tests are the thing that notices.
    /// </summary>
    public class TranslationCompletenessTests
    {
        public static TheoryData<string, string> ResourceFiles => new()
        {
            { "EEaseWebAPI.Application.Resources.AppMessages", typeof(AppMessages).Assembly.FullName! },
            { "EEaseWebAPI.Application.Resources.ValidationMessages", typeof(ValidationMessages).Assembly.FullName! },
            { "EEaseWebAPI.API.Resources.ErrorMessages", typeof(ErrorMessages).Assembly.FullName! },
        };

        [Theory]
        [MemberData(nameof(ResourceFiles))]
        public void Every_english_entry_has_a_turkish_translation(string baseName, string assemblyName)
        {
            var (english, turkish) = Read(baseName, assemblyName);

            english.Keys.Should().NotBeEmpty();
            turkish.Keys.Should().BeEquivalentTo(english.Keys);
        }

        [Theory]
        [MemberData(nameof(ResourceFiles))]
        public void No_translation_was_left_as_a_copy_of_the_english_text(string baseName, string assemblyName)
        {
            var (english, turkish) = Read(baseName, assemblyName);

            // An entry that reads the same in both languages is almost always English that
            // somebody pasted in and moved on from.
            var untranslated = english
                .Where(entry => turkish.TryGetValue(entry.Key, out var value) && value == entry.Value)
                .Select(entry => entry.Key)
                .ToList();

            untranslated.Should().BeEmpty();
        }

        private static (Dictionary<string, string> English, Dictionary<string, string> Turkish)
            Read(string baseName, string assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            var manager = new ResourceManager(baseName, assembly);

            return (ToDictionary(manager, new CultureInfo("en"), tryParents: true),
                    ToDictionary(manager, new CultureInfo("tr"), tryParents: false));
        }

        private static Dictionary<string, string> ToDictionary(
            ResourceManager manager, CultureInfo culture, bool tryParents)
        {
            var set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: tryParents);

            set.Should().NotBeNull($"the {culture.Name} resources should be compiled into the assembly");

            return set!.Cast<DictionaryEntry>()
                .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
        }
    }
}
