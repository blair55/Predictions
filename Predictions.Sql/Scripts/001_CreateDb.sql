/****** Object:  Table [dbo].[Fixtures]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Fixtures](
	[FixtureId] [uniqueidentifier] NOT NULL,
	[GameWeekId] [uniqueidentifier] NOT NULL,
	[KickOff] [datetime] NOT NULL,
	[HomeTeamName] [varchar](100) NOT NULL,
	[AwayTeamName] [varchar](100) NOT NULL,
	[HomeTeamScore] [int] NULL,
	[AwayTeamScore] [int] NULL,
 CONSTRAINT [PK_Fixtures] PRIMARY KEY CLUSTERED 
(
	[FixtureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[GameWeeks]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GameWeeks](
	[GameWeekId] [uniqueidentifier] NOT NULL,
	[SeasonId] [uniqueidentifier] NOT NULL,
	[GameWeekNumber] [int] NOT NULL,
	[GameWeekDescription] [varchar](1000) NOT NULL,
 CONSTRAINT [PK_GameWeeks] PRIMARY KEY CLUSTERED 
(
	[GameWeekId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LeaguePlayerBridge]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeaguePlayerBridge](
	[LeagueId] [uniqueidentifier] NOT NULL,
	[PlayerId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_LeaguePlayerBridge] PRIMARY KEY CLUSTERED 
(
	[LeagueId] ASC,
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Leagues]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Leagues](
	[LeagueId] [uniqueidentifier] NOT NULL,
	[LeagueShareableId] [nchar](8) NOT NULL,
	[LeagueName] [varchar](100) NOT NULL,
	[LeagueAdminPlayerId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Leagues_1] PRIMARY KEY CLUSTERED 
(
	[LeagueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Players]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Players](
	[PlayerId] [uniqueidentifier] NOT NULL,
	[ExternalLoginId] [varchar](100) NOT NULL,
	[ExternalLoginProvider] [varchar](100) NOT NULL,
	[PlayerName] [varchar](1000) NOT NULL,
	[IsAdmin] [bit] NOT NULL,
	[Email] [varchar](500) NOT NULL,
 CONSTRAINT [PK_Players] PRIMARY KEY CLUSTERED 
(
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ExternalLogin] UNIQUE NONCLUSTERED 
(
	[ExternalLoginId] ASC,
	[ExternalLoginProvider] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Predictions]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Predictions](
	[PredictionId] [uniqueidentifier] NOT NULL,
	[FixtureId] [uniqueidentifier] NOT NULL,
	[PlayerId] [uniqueidentifier] NOT NULL,
	[HomeTeamScore] [int] NOT NULL,
	[AwayTeamScore] [int] NOT NULL,
 CONSTRAINT [PK_Predictions] PRIMARY KEY CLUSTERED 
(
	[PredictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [CK_Predictions_FixtureId_PlayerId] UNIQUE NONCLUSTERED 
(
	[FixtureId] ASC,
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Seasons]    Script Date: 12/07/2015 00:35:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Seasons](
	[SeasonId] [uniqueidentifier] NOT NULL,
	[SeasonYear] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Seasons] PRIMARY KEY CLUSTERED 
(
	[SeasonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Fixtures]  WITH CHECK ADD  CONSTRAINT [FK_Fixtures_GameWeekId] FOREIGN KEY([GameWeekId])
REFERENCES [dbo].[GameWeeks] ([GameWeekId])
GO
ALTER TABLE [dbo].[Fixtures] CHECK CONSTRAINT [FK_Fixtures_GameWeekId]
GO
ALTER TABLE [dbo].[GameWeeks]  WITH CHECK ADD  CONSTRAINT [FK_GameWeeks_SeasonId] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([SeasonId])
GO
ALTER TABLE [dbo].[GameWeeks] CHECK CONSTRAINT [FK_GameWeeks_SeasonId]
GO
ALTER TABLE [dbo].[LeaguePlayerBridge]  WITH CHECK ADD  CONSTRAINT [FK_LeaguePlayerBridge_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO
ALTER TABLE [dbo].[LeaguePlayerBridge] CHECK CONSTRAINT [FK_LeaguePlayerBridge_Leagues]
GO
ALTER TABLE [dbo].[LeaguePlayerBridge]  WITH CHECK ADD  CONSTRAINT [FK_LeaguePlayerBridge_Players] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Players] ([PlayerId])
GO
ALTER TABLE [dbo].[LeaguePlayerBridge] CHECK CONSTRAINT [FK_LeaguePlayerBridge_Players]
GO
ALTER TABLE [dbo].[Leagues]  WITH CHECK ADD  CONSTRAINT [FK_Leagues_Players] FOREIGN KEY([LeagueAdminPlayerId])
REFERENCES [dbo].[Players] ([PlayerId])
GO
ALTER TABLE [dbo].[Leagues] CHECK CONSTRAINT [FK_Leagues_Players]
GO
ALTER TABLE [dbo].[Predictions]  WITH CHECK ADD  CONSTRAINT [FK_Predictions_Fixtures] FOREIGN KEY([FixtureId])
REFERENCES [dbo].[Fixtures] ([FixtureId])
GO
ALTER TABLE [dbo].[Predictions] CHECK CONSTRAINT [FK_Predictions_Fixtures]
GO
ALTER TABLE [dbo].[Predictions]  WITH CHECK ADD  CONSTRAINT [FK_Predictions_Players] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Players] ([PlayerId])
GO
ALTER TABLE [dbo].[Predictions] CHECK CONSTRAINT [FK_Predictions_Players]
GO
