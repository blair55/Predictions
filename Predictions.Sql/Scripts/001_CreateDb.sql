/****** Object:  Table [dbo].[Fixtures]    Script Date: 12/07/2015 12:44:30 ******/
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
	[AwayTeamScore] [int] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[GameWeeks]    Script Date: 12/07/2015 12:44:30 ******/
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
	[GameWeekDescription] [varchar](1000) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LeaguePlayerBridge]    Script Date: 12/07/2015 12:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeaguePlayerBridge](
	[LeagueId] [uniqueidentifier] NOT NULL,
	[PlayerId] [uniqueidentifier] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Leagues]    Script Date: 12/07/2015 12:44:30 ******/
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
	[LeagueAdminId] [uniqueidentifier] NOT NULL,
	[LeagueIsDeleted] [bit] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Players]    Script Date: 12/07/2015 12:44:30 ******/
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
	[Email] [varchar](500) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Predictions]    Script Date: 12/07/2015 12:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Predictions](
	[PredictionId] [uniqueidentifier] NOT NULL,
	[FixtureId] [uniqueidentifier] NOT NULL,
	[PlayerId] [uniqueidentifier] NOT NULL,
	[HomeTeamScore] [int] NOT NULL,
	[AwayTeamScore] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SchemaVersions]    Script Date: 12/07/2015 12:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchemaVersions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScriptName] [nvarchar](255) NOT NULL,
	[Applied] [datetime] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Seasons]    Script Date: 12/07/2015 12:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Seasons](
	[SeasonId] [uniqueidentifier] NOT NULL,
	[SeasonYear] [varchar](100) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [PK_Fixtures]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Fixtures] ADD  CONSTRAINT [PK_Fixtures] PRIMARY KEY CLUSTERED 
(
	[FixtureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_GameWeeks]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[GameWeeks] ADD  CONSTRAINT [PK_GameWeeks] PRIMARY KEY CLUSTERED 
(
	[GameWeekId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_LeaguePlayerBridge]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[LeaguePlayerBridge] ADD  CONSTRAINT [PK_LeaguePlayerBridge] PRIMARY KEY CLUSTERED 
(
	[LeagueId] ASC,
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_Leagues_1]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Leagues] ADD  CONSTRAINT [PK_Leagues_1] PRIMARY KEY CLUSTERED 
(
	[LeagueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_Players]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Players] ADD  CONSTRAINT [PK_Players] PRIMARY KEY CLUSTERED 
(
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_Predictions]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Predictions] ADD  CONSTRAINT [PK_Predictions] PRIMARY KEY CLUSTERED 
(
	[PredictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_SchemaVersions_Id]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[SchemaVersions] ADD  CONSTRAINT [PK_SchemaVersions_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PK_Seasons]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Seasons] ADD  CONSTRAINT [PK_Seasons] PRIMARY KEY CLUSTERED 
(
	[SeasonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_ExternalLogin]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Players] ADD  CONSTRAINT [IX_ExternalLogin] UNIQUE NONCLUSTERED 
(
	[ExternalLoginId] ASC,
	[ExternalLoginProvider] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [CK_Predictions_FixtureId_PlayerId]    Script Date: 12/07/2015 12:44:30 ******/
ALTER TABLE [dbo].[Predictions] ADD  CONSTRAINT [CK_Predictions_FixtureId_PlayerId] UNIQUE NONCLUSTERED 
(
	[FixtureId] ASC,
	[PlayerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
ALTER TABLE [dbo].[Leagues]  WITH CHECK ADD  CONSTRAINT [FK_Leagues_Players] FOREIGN KEY([LeagueAdminId])
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
