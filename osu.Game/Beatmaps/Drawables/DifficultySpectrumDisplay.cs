// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultySpectrumDisplay : CompositeDrawable
    {
        public DifficultySpectrumDisplay(IBeatmapSetInfo beatmapSet)
        {
            AutoSizeAxes = Axes.Both;

            FillFlowContainer<RulesetDifficultyGroup> flow;

            InternalChild = flow = new FillFlowContainer<RulesetDifficultyGroup>
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 0),
                Direction = FillDirection.Horizontal,
            };

            // matching web: https://github.com/ppy/osu-web/blob/d06d8c5e735eb1f48799b1654b528e9a7afb0a35/resources/assets/lib/beatmapset-panel.tsx#L127
            bool collapsed = beatmapSet.Beatmaps.Count() > 12;

            foreach (var rulesetGrouping in beatmapSet.Beatmaps.GroupBy(beatmap => beatmap.Ruleset.OnlineID))
            {
                flow.Add(new RulesetDifficultyGroup(rulesetGrouping.Key, rulesetGrouping, collapsed));
            }
        }

        private class RulesetDifficultyGroup : FillFlowContainer
        {
            private readonly int rulesetId;
            private readonly IEnumerable<IBeatmapInfo> beatmapInfos;
            private readonly bool collapsed;

            public RulesetDifficultyGroup(int rulesetId, IEnumerable<IBeatmapInfo> beatmapInfos, bool collapsed)
            {
                this.rulesetId = rulesetId;
                this.beatmapInfos = beatmapInfos;
                this.collapsed = collapsed;
            }

            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                AutoSizeAxes = Axes.Both;
                Spacing = new Vector2(1, 0);
                Direction = FillDirection.Horizontal;

                var icon = rulesets.GetRuleset(rulesetId)?.CreateInstance()?.CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };
                Add(icon.With(i =>
                {
                    i.Size = new Vector2(14);
                    i.Anchor = i.Origin = Anchor.Centre;
                }));

                if (!collapsed)
                {
                    foreach (var beatmapInfo in beatmapInfos.OrderBy(bi => bi.StarRating))
                        Add(new DifficultyDot(beatmapInfo.StarRating));
                }
                else
                {
                    Add(new OsuSpriteText
                    {
                        Text = beatmapInfos.Count().ToLocalisableString(@"N0"),
                        Font = OsuFont.Default.With(size: 12),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Bottom = 1 }
                    });
                }
            }
        }

        private class DifficultyDot : CircularContainer
        {
            private readonly double starDifficulty;

            public DifficultyDot(double starDifficulty)
            {
                this.starDifficulty = starDifficulty;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Width = 4;
                Height = 8;
                Anchor = Origin = Anchor.Centre;
                Masking = true;

                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.ForStarDifficulty(starDifficulty)
                };
            }
        }
    }
}
