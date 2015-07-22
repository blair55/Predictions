/****** Object:  Table [dbo].[DoubleDowns]    Script Date: 22/07/2015 21:40:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DoubleDowns](
	[PlayerId] [uniqueidentifier] NOT NULL,
	[GameWeekId] [uniqueidentifier] NOT NULL,
	[PredictionId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_DoubleDowns] PRIMARY KEY CLUSTERED 
(
	[PlayerId] ASC,
	[GameWeekId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Index [IX_DoubleDowns_PlayerId_PredictionId]    Script Date: 22/07/2015 21:40:19 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_DoubleDowns_PlayerId_PredictionId] ON [dbo].[DoubleDowns]
(
	[PlayerId] ASC,
	[PredictionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DoubleDowns]  WITH CHECK ADD  CONSTRAINT [FK_DoubleDowns_GameWeeks] FOREIGN KEY([GameWeekId])
REFERENCES [dbo].[GameWeeks] ([GameWeekId])
GO
ALTER TABLE [dbo].[DoubleDowns] CHECK CONSTRAINT [FK_DoubleDowns_GameWeeks]
GO
ALTER TABLE [dbo].[DoubleDowns]  WITH CHECK ADD  CONSTRAINT [FK_DoubleDowns_Players] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Players] ([PlayerId])
GO
ALTER TABLE [dbo].[DoubleDowns] CHECK CONSTRAINT [FK_DoubleDowns_Players]
GO
