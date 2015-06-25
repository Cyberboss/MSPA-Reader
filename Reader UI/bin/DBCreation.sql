
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
CREATE TABLE [dbo].[Dialog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[page_id] [int] NOT NULL,
	[x2] [bit] NOT NULL,
	[isNarrative] [bit] NOT NULL,
	[isImg] [bit] NOT NULL,
	[text] [nvarchar](max) NULL,
	[colour] [nchar](7) NULL,
 CONSTRAINT [PK_Dialog] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[Links](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[page_id] [int] NOT NULL,
	[linked_page_id] [int] NULL,
	[link_text] [nvarchar](200) NULL,
 CONSTRAINT [PK_Links] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[PageMeta](
	[page_id] [int] NOT NULL,
	[x2] [bit] NOT NULL,
	[title] [nvarchar](max) NULL,
	[promptType] [nvarchar](100) NULL,
 CONSTRAINT [PK_PageMeta] PRIMARY KEY CLUSTERED 
(
	[page_id],
	[x2]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[PagesArchived](
	[page_id] [int] NOT NULL,
	[x2] [bit] NOT NULL,
 CONSTRAINT [PK_PagesArchived] PRIMARY KEY CLUSTERED 
(
	[page_id],
	[x2]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[Resources](
	[id] [int] NOT NULL IDENTITY (1,1),
	[page_id] [int] NOT NULL,
	[data] [varbinary](max) NULL,
	[original_filename] [nvarchar](max) NULL,
	[title_text] [nvarchar](max) NULL,
 CONSTRAINT [PK_Resources] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[SpecialText](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[dialog_id] [int] NOT NULL,
	[underline] [bit] NOT NULL,
	[colour] [nchar](7) NOT NULL,
	[sbegin] [int] NOT NULL,
	[length] [int] NOT NULL,
 CONSTRAINT [PK_SpecialText] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[SpecialText]  WITH CHECK ADD  CONSTRAINT [FK_SpecialText_Dialog] FOREIGN KEY([dialog_id])
REFERENCES [dbo].[Dialog] ([id]);
ALTER TABLE [dbo].[SpecialText] CHECK CONSTRAINT [FK_SpecialText_Dialog];
