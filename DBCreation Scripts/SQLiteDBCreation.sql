CREATE TABLE DBVersion (dbverison INTEGER PRIMARY KEY);
CREATE TABLE PageMeta (page_id INTEGER, x2 BOOLEAN, title TEXT, promptType TEXT, headerAltText TEXT, PRIMARY KEY (page_id, x2));
CREATE TABLE Dialog (id INTEGER PRIMARY KEY AUTOINCREMENT, page_id INTEGER REFERENCES PageMeta (page_id), x2 BOOLEAN, isNarrative BOOLEAN, isImg BOOLEAN, text TEXT, colour TEXT, precedingLineBreaks INTEGER);
CREATE TABLE Links (id INTEGER PRIMARY KEY AUTOINCREMENT, page_id INTEGER, x2 BOOLEAN, linked_page_id INTEGER, link_text TEXT);
CREATE TABLE PagesArchived (page_id INTEGER, x2 BOOLEAN, PRIMARY KEY (page_id, x2));
CREATE TABLE Resources (id INTEGER PRIMARY KEY AUTOINCREMENT, page_id INTEGER, x2 BOOLEAN, data BLOB, original_filename TEXT, title_text TEXT, isInPesterLog BOOLEAN);
CREATE TABLE SpecialText (id INTEGER PRIMARY KEY AUTOINCREMENT, dialog_id INTEGER, underline BOOLEAN, colour TEXT, sbegin INTEGER, length INTEGER, isImg BOOLEAN);