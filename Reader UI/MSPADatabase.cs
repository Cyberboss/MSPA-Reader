using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Common;
using SQLite.CodeFirst;

namespace Reader_UI
{
    class MSPADatabase : DbContext
    {
        public static MSPADatabase Initialize(DbConnection existingConnection, bool r, DatabaseManager.DBType dbtype)
        {
            if(dbtype == DatabaseManager.DBType.MYSQL)
                DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
            var db = new MSPADatabase(existingConnection, r, dbtype);
            db.Database.Initialize(false);
            return db;
        }
        DbContextTransaction transaction = null;
        private readonly bool isSqlite, reset;
        public MSPADatabase(DbConnection existingConnection, bool r, DatabaseManager.DBType dbtype)
            : base(existingConnection, true)
        {
            isSqlite = dbtype == DatabaseManager.DBType.SQLITE;
            reset = r;
            if (reset)
                Database.SetInitializer<MSPADatabase>(new DropCreateDatabaseAlways<MSPADatabase>());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (isSqlite)
                if (reset)
                    Database.SetInitializer<MSPADatabase>(new SqliteDropCreateDatabaseAlways<MSPADatabase>(Database.Connection.ConnectionString,modelBuilder));
                else
                    Database.SetInitializer<MSPADatabase>(new SqliteCreateDatabaseIfNotExists<MSPADatabase>(Database.Connection.ConnectionString, modelBuilder));
        }
        public MSPADatabase(DbConnection existingConnection)
            : base(existingConnection, false)
        {}
        public void Transact()
        {
            if (transaction != null)
                throw new Exception();
            transaction = Database.BeginTransaction();
        }
        public void Commit()
        {
            SaveChanges();
            transaction.Commit();
            transaction = null;
        }
        public void Rollback()
        {
            transaction.Rollback();
            transaction = null;
        }
        public class Version
        {
            public long Id { get; set; }
            public int DatabaseVersion { get; set; }
        }
        public class Archives
        {
            public long Id { get; set; }
            public int pageId { get; set; }
            public bool x2 { get; set; }
            public Archives() { }
            public Archives(int pid, bool x)
            {
                pageId = pid;
                x2 = x;
            }
        }
        public class Resource
        {
            public long Id { get; set; }
            public int pageId { get; set; }
            public bool x2 { get; set; }
            public byte[] data { get; set; }
            public string originalFileName { get; set; }
            public string titleText { get; set; }
            public bool isInPesterLog { get; set; }
            public Resource() { }
            public Resource(Parser.Resource pObj, int page, bool ix2)
            {
                pageId = page;
                x2 = ix2;
                data = pObj.data;
                originalFileName = pObj.originalFileName;
                titleText = pObj.titleText;
                isInPesterLog = pObj.isInPesterLog;
            }
        }
        public class Text
        {
            public class ScriptLine
            {
                public class SpecialSubText
                {
                    public long Id { get; set; }
                    public int begin { get; set; }
                    public int length { get; set; }
                    public bool isImg { get; set; }
                    public bool underlined { get; set; }
                    public string colour { get; set; }

                    public SpecialSubText() { }
                    public SpecialSubText(Parser.Text.ScriptLine.SpecialSubText sst)
                    {
                        begin = sst.begin;
                        length = sst.length;
                        isImg = sst.isImg;
                        underlined = sst.underlined;
                        colour = sst.colour;
                    }
                    public Parser.Text.ScriptLine.SpecialSubText ToParserObject()
                    {
                        if (isImg)
                            return new Parser.Text.ScriptLine.SpecialSubText(begin, length, colour);
                        else
                            return new Parser.Text.ScriptLine.SpecialSubText(begin, length, underlined, colour);
                    }
                }
                public long Id { get; set; }
                public bool isImg { get; set; }
                public string hexColour { get; set; }
                public string text { get; set; }
                public int precedingLineBreaks { get; set; }
                public IList<SpecialSubText> subTexts { get; set; }

                public ScriptLine() { }
                public ScriptLine(Parser.Text.ScriptLine sl) {

                    isImg = sl.isImg;
                    hexColour = sl.hexColour;
                    text = sl.text;
                    precedingLineBreaks = sl.precedingLineBreaks;
                    if (sl.subTexts != null)
                    {
                        subTexts = new List<SpecialSubText>();
                        foreach (var sst in sl.subTexts)
                            subTexts.Add(new SpecialSubText(sst));
                    }
                }
                public Parser.Text.ScriptLine ToParserObject()
                {
                    Parser.Text.ScriptLine sl;
                    if (!isImg)
                        sl = new Parser.Text.ScriptLine(hexColour, text, precedingLineBreaks);
                    else
                        sl = new Parser.Text.ScriptLine(text, precedingLineBreaks);

                    var lines = new List<Parser.Text.ScriptLine.SpecialSubText>();
                    if(subTexts != null)
                        foreach(var sst in subTexts)
                            lines.Add(sst.ToParserObject());
                    sl.subTexts = lines.ToArray();

                    return sl;
                }
            }
            public long Id { get; set; }
            public int pageId { get; set; }
            public bool x2 { get; set; }
            public string title { get; set; }
            public string promptType { get; set; }
            public string altText { get; set; }
            public ScriptLine narr { get; set; }
            public IList<ScriptLine> lines { get; set; }
            public Text() { }
            public Text(Parser.Text pObj, int pageno, bool ix2)
            {
                pageId = pageno;
                x2 = ix2;
                title = pObj.title;
                promptType = pObj.promptType;
                altText = pObj.altText;
                if(pObj.narr != null)
                    narr = new ScriptLine(pObj.narr);
                if (pObj.lines != null)
                {
                    lines = new List<ScriptLine>();
                    foreach (var l in pObj.lines)
                        lines.Add(new ScriptLine(l));
                }

            }
            public Parser.Text.ScriptLine[] GetScriptLines()
            {
                var line = new List<Parser.Text.ScriptLine>();
                if(lines != null)
                    foreach(var sst in lines)
                        line.Add(sst.ToParserObject());
                return line.ToArray();
            }
        }
        public class Link
        {
            public long Id { get; set; }
            public int pageId { get; set; }
            public int linkTo { get; set; }
            public string originalText { get; set; }
            public Link() { }
            public Link(Parser.Link pObj, int pageno)
            {
                pageId = pageno;
                linkTo = pObj.pageNumber;
                originalText = pObj.originalText;
            }
        }
        public DbSet<Version> Versions { get; set; }
        public DbSet<Archives> ArchivedPages { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Text> PageMeta { get; set; }
        public DbSet<Link> Links { get; set; }

        public void Prune(int pageno)
        {
            Transact();
            try
            {
                var selectedRes = (from b in Resources
                                   where b.pageId == pageno
                                   select b);
                var selectedMeta2 = (from b in PageMeta
                                     where b.pageId == pageno
                                     select b).Include(m => m.narr).Include(m => m.lines).Include(m => m.narr.subTexts).Include(m => m.lines.Select(l => l.subTexts));
                var selectedLinks = (from b in Links
                                     where b.pageId == pageno
                                     select b).ToList();
                var selectedPage = (from b in ArchivedPages where b.pageId == pageno select b);

                foreach (var r in selectedRes)
                    Resources.Remove(r);
                foreach (var l in selectedLinks)
                    Links.Remove(l);
                foreach (var p in selectedPage)
                    ArchivedPages.Remove(p);
                foreach (var selectedMeta in selectedMeta2)
                {
                    if (selectedMeta.narr != null)
                    {
                        foreach (var sst in selectedMeta.narr.subTexts)
                        {
                            selectedMeta.narr.subTexts.Remove(sst);
                        }
                    }
                    else
                        foreach (var l in selectedMeta.lines)
                        {
                            foreach (var sst in l.subTexts)
                            {
                                l.subTexts.Remove(sst);
                            }
                        }
                    PageMeta.Remove(selectedMeta);
                }
                Commit();
            }
            catch
            {
                Rollback();
                System.Diagnostics.Debugger.Break();
            }
        }
        public Writer.Page ToWriterObject(int pageNo, bool x2)
        {

            var page = new Writer.Page(pageNo);

            var selectedRes = (from b in Resources
                              where b.pageId == pageNo && b.x2 == false
                               select b);
            var selectedMeta = (from b in PageMeta
                                where b.pageId == pageNo && b.x2 == false
                                select b).Include(m => m.narr).Include(m => m.lines).Include(m => m.narr.subTexts).Include(m => m.lines.Select(l => l.subTexts)).First();
            var selectedLinks = (from b in Links
                                where b.pageId == pageNo
                                 select b).ToList();

            List < Parser.Link> lnks = new List < Parser.Link>();
            foreach (var l in selectedLinks)
                lnks.Add(new Parser.Link(l.originalText, l.linkTo));
            page.links = lnks.ToArray();

            List<Parser.Resource> res = new List<Parser.Resource>();
            foreach (var r in selectedRes)
            {
                var tmp = new Parser.Resource(r.data, r.originalFileName, r.titleText);
                tmp.isInPesterLog = r.isInPesterLog;
                res.Add(tmp);
            }
            page.resources = res.ToArray();

            page.meta = new Parser.Text();
            page.meta.altText = selectedMeta.altText;
            page.meta.lines = selectedMeta.GetScriptLines();
            if (selectedMeta.narr != null)
                page.meta.narr = selectedMeta.narr.ToParserObject();
            else if (page.meta.lines == null)
                page.meta.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            page.meta.promptType = selectedMeta.promptType;
            page.meta.title = selectedMeta.title;

            if(x2){

                page.links2 = page.links;
                page.links = null;

                selectedRes = (from b in Resources
                              where b.pageId == pageNo && b.x2 == true
                               select b);

                selectedMeta = (from b in PageMeta
                                    where b.pageId == pageNo && b.x2 == true
                                select b).Include(m => m.narr).Include(m => m.lines).Include(m => m.narr.subTexts).Include(m => m.lines.Select(l => l.subTexts)).First();
                res.Clear();
                foreach (var r in selectedRes)
                {
                    var tmp = new Parser.Resource(r.data, r.originalFileName, r.titleText);
                    tmp.isInPesterLog = r.isInPesterLog;
                    res.Add(tmp);
                }
                page.resources2 = res.ToArray();

                page.meta2 = new Parser.Text();
                page.meta2.altText = selectedMeta.altText;
                page.meta2.lines = selectedMeta.GetScriptLines();
                if (selectedMeta.narr != null)
                    page.meta2.narr = selectedMeta.narr.ToParserObject();
                else if (page.meta2.lines == null)
                    page.meta2.narr = new Parser.Text.ScriptLine("#000000", "", 0);
                page.meta2.promptType = selectedMeta.promptType;
                page.meta2.title = selectedMeta.title;
            }

            return page;
        }

    }
}
