using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using Shockah.Shared;
using Nanoray.EnumByNameSourceGenerator;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using APurpleApple.ShipPartExpansion.ExternalAPIs;
using Nanoray.Pintail;

namespace APurpleApple.ShipPartExpansion;

public sealed class PMod : SimpleMod
{
    internal static PMod Instance { get; private set; } = null!;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }

    public static Dictionary<string, ISpriteEntry> sprites = new();
    public static Dictionary<string, IStatusEntry> statuses = new();
    public static Dictionary<string, IPartEntry> parts = new();
    public static Dictionary<string, TTGlossary> glossaries = new();
    public static Dictionary<string, ICharacterAnimationEntryV2> animations = new();
    public static Dictionary<string, IArtifactEntry> artifacts = new();
    public static Dictionary<string, ICardEntry> cards = new();
    public static Dictionary<string, ICharacterEntryV2> characters = new();
    public static Dictionary<string, IShipEntry> ships = new();
    public static Dictionary<string, IDeckEntry> decks = new();

    public static string Name { get; } = typeof(PMod).Namespace!;
    public static IKokoroApi? kokoroApi { get; private set; }
    public static Assembly? kokoroAssembly { get; private set; }

    public static List<Tuple<Type, PType>> cardActionLooksForType = new();

    public static ApiImplementation Api = new ApiImplementation();

    public override object? GetApi(IModManifest requestingMod) => new ApiImplementation();

    public void RegisterSprite(string key, string fileName, IPluginPackage<IModManifest> package)
    {
        sprites.Add(key, Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/" + fileName)));
    }

    private void Patch()
    {
        Harmony harmony = new("APurpleApple.ShipPartExpansion");
        harmony.PatchAll();
        CustomTTGlossary.ApplyPatches(harmony);
    }

    public PMod(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;
        
        this.AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        this.Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(this.AnyLocalizations)
        );

        Patch();

        helper.Events.OnModLoadPhaseFinished += (object? sender, ModLoadPhase e) => {
            if (e == ModLoadPhase.AfterDbInit)
            {
                //kokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro");
            }
        };

    }
}
