USE [MSPATest]
GO

/****** Object:  Table [dbo].[Links]    Script Date: 22/06/2015 1:05:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Links](
	[id] [int] NOT NULL,
	[page_id] [int] NOT NULL,
	[linked_page_id] [int] NULL,
	[link_text] [nvarchar](50) NULL,
 CONSTRAINT [PK_Links] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

