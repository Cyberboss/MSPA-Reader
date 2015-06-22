USE [MSPATest]
GO

/****** Object:  Table [dbo].[Conversations]    Script Date: 22/06/2015 1:04:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Conversations](
	[id] [int] NOT NULL,
	[page_id] [int] NOT NULL,
	[text] [nvarchar](max) NULL,
 CONSTRAINT [PK_Conversations] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


