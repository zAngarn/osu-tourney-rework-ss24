// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Schedule
{
    public partial class ScheduleScreen : TournamentScreen
    {
        private readonly BindableList<TournamentMatch> allMatches = new BindableList<TournamentMatch>();
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();
        private Container mainContainer = null!;
        private LadderInfo ladder = null!;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            this.ladder = ladder;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("schedule")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(100) { Bottom = 50 },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DrawableTournamentHeaderText(),
                                            new Container
                                            {
                                                Margin = new MarginPadding { Top = 40 },
                                                AutoSizeAxes = Axes.Both,
                                            },
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    mainContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            allMatches.BindTo(ladder.Matches);
            allMatches.BindCollectionChanged((_, _) => refresh());

            currentMatch.BindTo(ladder.CurrentMatch);
            currentMatch.BindValueChanged(_ => refresh(), true);
        }

        private void refresh()
        {
            const int days_for_displays = 4;

            IEnumerable<ConditionalTournamentMatch> conditionals =
                allMatches
                    .Where(m => !m.Completed.Value && (m.Team1.Value == null || m.Team2.Value == null) && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .SelectMany(m => m.ConditionalMatches.Where(cp => m.Acronyms.TrueForAll(a => cp.Acronyms.Contains(a))));

            IEnumerable<TournamentMatch> upcoming =
                allMatches
                    .Where(m => !m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .Concat(conditionals)
                    .OrderBy(m => m.Date.Value)
                    .Take(8);

            var recent =
                allMatches
                    .Where(m => m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .OrderByDescending(m => m.Date.Value)
                    .Take(8);

            ScheduleContainer comingUpNext;

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.74f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleContainer("partidos recientes")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.4f,
                                    ChildrenEnumerable = recent.Select(p => new ScheduleMatch(p))
                                },
                                new ScheduleContainer("próximamente")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p))
                                },
                            }
                        }
                    },
                    comingUpNext = new ScheduleContainer("próximo partido")
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.25f,
                    }
                }
            };

            if (currentMatch.Value != null)
            {
                comingUpNext.Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(30),
                    Children = new Drawable[]
                    {
                        new ScheduleMatch(currentMatch.Value, false)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TournamentSpriteTextWithBackground(currentMatch.Value.Round.Value?.Name.Value ?? string.Empty)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Scale = new Vector2(0.75f),
                            Colour = Colour4.FromHex("#d6c568")
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = currentMatch.Value.Team1.Value?.FullName + " vs " + currentMatch.Value.Team2.Value?.FullName,
                            Font = OsuFont.Poppins.With(size: 40, weight: FontWeight.Light)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                new ScheduleMatchDate(currentMatch.Value.Date.Value)
                                {
                                    Font = OsuFont.Poppins.With(size: 40, weight: FontWeight.Light),
                                    Colour = Colour4.FromHex("#91908a")
                                }
                            }
                        },
                    }
                };
            }
        }

        public partial class ScheduleMatch : DrawableTournamentMatch
        {
            public ScheduleMatch(TournamentMatch match, bool showTimestamp = true)
                : base(match)
            {
                Flow.Direction = FillDirection.Horizontal;

                Scale = new Vector2(0.8f);

                bool conditional = match is ConditionalTournamentMatch;

                if (conditional)
                    Colour = OsuColour.Gray(0.5f);

                if (showTimestamp)
                {
                    AddInternal(new DrawableDate(Match.Date.Value)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Font = OsuFont.Poppins,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    });
                    AddInternal(new TournamentSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Text = match.Date.Value.ToUniversalTime().ToString("HH:mm UTC") + (conditional ? " (conditional)" : "")
                    });
                }
            }
        }

        public partial class ScheduleMatchDate : DrawableDate
        {
            public ScheduleMatchDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
                : base(date, textSize, italic)
            {
            }

            protected override string Format() => Date < DateTimeOffset.Now
                ? $"Comenzó {base.Format()}"
                : $"Comienza {base.Format()}";
        }

        public partial class ScheduleContainer : Container
        {
            protected override Container<Drawable> Content => content;

            private readonly FillFlowContainer content;

            public ScheduleContainer(string title)
            {
                Padding = new MarginPadding { Left = 60, Top = 10 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText {
                                Shadow = true,
                                Text = title.ToUpperInvariant(),
                                Font = OsuFont.Poppins.With(size: 40, weight: FontWeight.SemiBold),
                                Colour = Colour4.FromHex("#d6c14f")
                            },
                            content = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.Both,
                                Spacing = new Vector2(0, -6),
                                Margin = new MarginPadding(10)
                            },
                        }
                    },
                };
            }
        }
    }
}
