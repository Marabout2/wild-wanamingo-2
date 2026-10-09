// Ported from Nuclear 14 (https://github.com/Vault-Overseers/nuclear-14) via Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_N14/Special/Speech/Systems/LowIntelligenceAccentSystem.cs

using System.Text.RegularExpressions;
using Content.Shared.Speech;
using Content.Server._Nuclear14.Special.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server._Nuclear14.Special.Speech.EntitySystems
{
    public sealed partial class LowIntelligenceAccentSystem : EntitySystem
    {
        [Dependency] private IRobustRandom _random = default!;

        private static readonly Regex yesRegex = new(@"(?:yes\b|yep\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex noRegex = new(@"(?:no\b|nope\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ingRegex = new(@"(?:ing\b|in\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ilityRegex = new(@"(?:ility\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex tionRegex = new(@"(?:tion\b|tions\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex lettersRegex = new(@"[ao]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void Initialize()
        {
            SubscribeLocalEvent<LowIntelligenceAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            message = message.Trim();

            message = yesRegex.Replace(message, "duh");
            message = noRegex.Replace(message, "nungh");
            message = ingRegex.Replace(message, "uuuh");
            message = ilityRegex.Replace(message, "uuity");
            message = tionRegex.Replace(message, "tuun");

            message = lettersRegex.Replace(message, "u");

            return message;
        }

        private void OnAccent(EntityUid uid, LowIntelligenceAccentComponent component, ref AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
