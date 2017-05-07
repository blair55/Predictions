CREATE NONCLUSTERED INDEX [NCIX_Fixtures_FixtureId] ON [dbo].[Fixtures]
(
	[FixtureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE NONCLUSTERED INDEX NCIX_Fixtures_FixtureId_GameweekId
ON [dbo].[Fixtures] ([GameWeekId])
INCLUDE ([FixtureId])
GO