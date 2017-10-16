/*
SQLDOM HTML parser and DOM tools for MSSQL.
https://sourceforge.net/projects/sqldom/

Parses HTML from a string or from a URL into a DOM (document object model)
implemented with SQL tables.  Provides routines to manipulate the DOM data
and to render the DOM data back to HTML.

You may safely run this entire script:  it does not make any changes to any
SQL user databases.  It only creates some local temporary tables and temporary
stored procedures, and prints out a string with some instructions.

Requires Microsoft SQL 2005 or later.

Copyright (C) 2012 David B. Rueter (drueter@assyst.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

HISTORY

Version .927 3/21/2013
  Added @HUIDLike as optional parameters for #spgetDOMHTML to allow
  retrieving a subset of the HTML (where HUID LIKE @HUIDLike + '%').  Added
  SET NOCOUNT ON to avoid performance hit if the caller had not set this option.
  
Version .926 10/17/2012
  Added @User and @Password as optional parameters for sdom.sputilGetHTTP to allow
  use of HTTP authentication
  
Version .925 10/9/2012
  Fixed a problem with parsing script tags in certain cases due to a bug
  introduced by optimizations in version .918  Added table #tblDOMHierarchy
  that is populated by sdom.spgetDOM (to avoid needing to insert results of sdom.spgetDOM
  into a user-created temp table to utilize the HUID field when joining to
  DOM rows).  Made slight improvement to quote detection logic.  Added procedure
  sdom.spgetText for convenience in getting text values once sdom.spgetDOM has been called.

Version .924 4/24/2012
  Fixed attribute handling to support attributes without values (such as <
  option selected>My Option</option>)  Thanks to JMelin for reporting.
  
Version .923 4/3/2012
  Fixed additional bug in sdom.spgetDOM pertaining to getting by selector.
  
Version .922 3/20/2012
  Fixed bug in sdom.spgetDOM pertaining to getting by selector.  Thanks to Brian Hurtt
  for reporting and providing correction.
  
Version .921 3/5/2012
  Added sdom.sputilConvertJSONToXML to convert JSON data to XML
  
Version .920 2/23/2012
  Refactor sdom.spgetDOMHTML to fix bugs, streamline
  
Version .919 2/21/2012
  Corrected problem with rendering HTML comments

Version .918  2/20/2012
  Removed dependencies on 3 UDF string helper functions
  Performance increase (approx. 23%)
  Clean up some comments
  
Version .917  2/19/2012
  Initial public version
  
*/

SET NOCOUNT ON

IF NOT EXISTS(SELECT * FROM sys.schemas WHERE name = 'sdom') BEGIN
  DECLARE @SQL varchar(MAX)
  SET @SQL = 'CREATE SCHEMA sdom'
  EXEC(@SQL)
END

IF OBJECT_ID('tempdb..#tblDOMDocs') IS NOT NULL BEGIN
  DROP TABLE #tblDOMDocs
END

IF OBJECT_ID('tempdb..#tblDOM') IS NOT NULL BEGIN
  DROP TABLE #tblDOM
END

IF OBJECT_ID('tempdb..#tblDOMAttribs') IS NOT NULL BEGIN
  DROP TABLE #tblDOMAttribs
END

IF OBJECT_ID('tempdb..#tblDOMStyles') IS NOT NULL BEGIN
  DROP TABLE #tblDOMStyles
END

IF OBJECT_ID('tempdb..#tblDOMHierarchy') IS NOT NULL BEGIN
  DROP TABLE #tblDOMHierarchy
END

IF OBJECT_ID('sdom.spactTrimWhitespace') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spactTrimWhitespace
END

IF OBJECT_ID('sdom.spgetLenNTW') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spgetLenNTW  
END

IF OBJECT_ID('sdom.spactDOMOpen') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spactDOMOpen
END

IF OBJECT_ID('sdom.spgetDOM') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spgetDOM
END

IF OBJECT_ID('sdom.spgetDOMHTML') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spgetDOMHTML
END

IF OBJECT_ID('sdom.spactDOMLoad') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spactDOMLoad
END

IF OBJECT_ID('sdom.spinsDOMNode') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spinsDOMNode
END

IF OBJECT_ID('sdom.spactDOMClear') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spactDOMClear
END

IF OBJECT_ID('sdom.spupdDOMAttribs') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spupdDOMAttribs
END

IF OBJECT_ID('sdom.spupdDOMStyles') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spupdDOMStyles
END

IF OBJECT_ID('sdom.sputilGetHTTP') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.sputilGetHTTP
END

IF OBJECT_ID('sdom.sputilConvertJSONToXML') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.sputilConvertJSONToXML
END

IF OBJECT_ID('sdom.spgetText') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spgetText
END

IF OBJECT_ID('sdom.spgetInitSession') IS NOT NULL BEGIN
  DROP PROCEDURE sdom.spgetInitSession
END

-- Spi: 21 statements.

/*
**************************************************************************************
TABLE #tblDOMDocs
Table #tblDOMDocs is for list of DOM documents (groups of tblDOM rows).
**************************************************************************************
*/

CREATE TABLE #tblDOMDocs(
DocID int identity PRIMARY KEY,
DateCreated datetime,
DocName varchar(128)
)

GO

/*
**************************************************************************************
TABLE  #tblDOM
Table #tblDOM  is for internal representation of the DOM data
**************************************************************************************
*/
CREATE TABLE #tblDOM (
  DEID int identity PRIMARY KEY,
  DocID int,  
  Tag varchar(MAX),
  ID varchar(512),
  Name varchar(512),  
  Class varchar(512),
  TextData varchar(MAX),
  OpenTagStartPos int,
  CloseTagEndPos int,
  ParentDEID int
)

CREATE INDEX ixDOMTable_ParentDEID ON #tblDOM (ParentDEID, DEID)
CREATE INDEX ixDOMTable_DocID_ParentDEID ON #tblDOM (DocID, ParentDEID, DEID)
CREATE INDEX ixDOMTable_DEID ON #tblDOM (DEID, DocID)

--NOTE: SQL 2008 introduced filtered indexes, which makes it easy to enforce
--unqique-but-nullable. If on SQL 2008 or greater AND you wish to enforce uniqueness
--of ID and Name attributes, uncomment the following two lines
--  CREATE UNIQUE INDEX tmpixDOMTable_ID ON #tblDOM (ID) INCLUDE (DEID) WHERE ID IS NOT NULL
--  CREATE UNIQUE INDEX tmpixDOMTable_Name ON #tblDOM (Name) INCLUDE (DEID) WHERE Name IS NOT NULL





/*
Note:
TextData will contain the data for the first text node (if any) under the tag.
Subsequent text nodes (if any) will be in their own #tblDOM row, with a null TAG
and referencing the original DEID in the ParentDEID column.
*/


GO

/*
**************************************************************************************
TABLE #DOMAttribs
Table #tblDOMAttribs is for internal representation of the DOM data--specifically,
for attributes of DOM elements
**************************************************************************************
*/
CREATE TABLE #tblDOMAttribs(
DOMAttribID int identity PRIMARY KEY,
DEID int,
Name varchar(512),
Value varchar(MAX)
)

CREATE UNIQUE INDEX uqDOMAttribs_DEID ON #tblDOMAttribs (DEID, Name)
CREATE INDEX ixDOMAttribs_DEID ON #tblDOMAttribs (DEID) INCLUDE (Name, Value)

GO
-- Spi: 32 statements.

/*
**************************************************************************************
TABLE #tblDOMStyles
Table #tblDOMAttribs is for internal representation of the DOM data--specifically,
for attributes of DOM elements
**************************************************************************************
*/
CREATE TABLE #tblDOMStyles(
DOMStyleID int identity PRIMARY KEY,
DEID int,
Name varchar(512),
Value varchar(MAX)
)

CREATE UNIQUE INDEX ixDOMStyles_ID ON #tblDOMStyles (DEID, Name)
CREATE INDEX ixDOMStyles_DEID ON #tblDOMStyles (DEID) INCLUDE (Name, Value)

GO


/*
**************************************************************************************
TABLE #tblDOMHierarchy
Table #tblDOMHierarchy is a table that automatically caches the output of sdom.spgetDOM
Most of the data is reduncant to what is in #tblDOM, but the 4 fields HUID,
SortHUID, DOMLevel, and Sequence are sufficiently useful to warrant this
duplication.  This table should be regarded as read-only and transitory.  Do not
update.
**************************************************************************************
*/
CREATE TABLE #tblDOMHierarchy(
  DEID int PRIMARY KEY,
  DocID int,
  Tag varchar(MAX),
  ID varchar(512),
  Name varchar(512),
  Class varchar(512),
  TextData varchar(MAX),
  OpenTagStartPos int,
  CloseTagEndPos int,
  ParentDEID int,
  --fields not present in #tblDOM:
  HUID varchar(900),
  SortHUID varchar(900),
  DOMLevel int,
  Sequence int,
  HasChild bit
)

CREATE INDEX ixDOMHierarchy_ParentDEID ON #tblDOMHierarchy (ParentDEID, DEID)
CREATE INDEX ixDOMHierarcy_HUID ON #tblDOMHierarchy (HUID) INCLUDE (DEID)
CREATE INDEX ixDOMHierarchy_SortHUID ON #tblDOMHierarchy (SortHUID) INCLUDE (DEID)
CREATE INDEX ixDOMHierarchy_Sequence ON #tblDOMHierarchy (Sequence, DEID)
GO

/*
**************************************************************************************
PROCEDURE sdom.spactTrimWhitespace
Simple helper function to do a left-trim  or right-trim of whitespace (spaces, tabs, 
carriage returns and linefeeds, and tabs).
I would really prefer this to be a function, but we are not allowed to create
temporary functions, and I do not want SQLDOM to require permanent database objects.
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spactTrimWhitespace
@S varchar(MAX) OUTPUT,
@DoLeft bit = 0,
@DoRight bit = 1
AS BEGIN
  SET NOCOUNT ON
  
  DECLARE @P int  
  
  IF @DoRight = 1 BEGIN
    --Right trim
    SET @P = LEN(@S + 'x') - 1 
    WHILE @P >= 1 BEGIN
      IF ISNULL(SUBSTRING(@S, @P, 1), ' ') IN (' ', CHAR(9), CHAR(10), CHAR(13)) BEGIN
        SET @P = @P - 1
      END
      ELSE BEGIN
        BREAK
      END
    END
    
    SET @S= LEFT(@S, @P)  
  END
  
  IF @DoLeft = 1 BEGIN  
    --Left trim
    SET @P = 1
    WHILE @P <= LEN(@S + 'x') - 1 BEGIN
      IF SUBSTRING(@S, @P, 1) IN  (' ', CHAR(9), CHAR(10), CHAR(13)) BEGIN
        SET @P = @P + 1
      END
      ELSE BEGIN
        BREAK
      END
    END
    
    SET @S = RIGHT(@S, LEN(@S + 'x') - 1 - @P + 1)    
  END
  
END  

GO
-- Spi: 45 statements.

/*
**************************************************************************************
PROCEDURE sdom.spgetLenNTW (no trailing whitespace)
Simple helper function to determine the length of a string after trimming all
trailing whitespace (spaces, tabs, carriage returns and linefeeds, and tabs).
I would really prefer this to be a function, but we are not allowed to create
temporary functions, and I do not want SQLDOM to require permanent database objects.
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spgetLenNTW
@S varchar(MAX),
@Len int OUTPUT
AS
BEGIN
  SET NOCOUNT ON
  
  SET @Len = LEN(@S + 'x') - 1

  DECLARE @Done bit
  SET @Done = 0
  
  WHILE @Done = 0 BEGIN
    IF (@Len > 0) AND (SUBSTRING(@S, @Len, 1) IN (CHAR(9), CHAR(10), CHAR(13), ' ')) BEGIN
      SET @Len = @Len - 1
    END
    ELSE BEGIN
      SET @Done = 1        
    END
  END
   
END

GO

/*
**************************************************************************************
PROCEDURE sdom.spactDOMOpen
Procedure sdom.spactDOMOpen verifies session and @DocID
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spactDOMOpen
@DocID int OUTPUT,
@CreateNew bit = 0
AS
BEGIN 
  SET NOCOUNT ON
  
  --Note:  if @DocID is provided, we trust it.  We don't validate that it exists
  --or that it belongs to this session.
  
  IF (@CreateNew = 1) BEGIN
    IF @DocID IS NOT NULL BEGIN
      RAISERROR('Error in sdom.spactDOMOpen:  Cannot specify @DocID if @CreateNew=1', 16, 1)
    END  
  
    INSERT INTO #tblDOMDocs (DateCreated)
    VALUES (GETDATE())
        
    SET @DocID = SCOPE_IDENTITY()
  END 
  ELSE BEGIN        
    IF @DocID IS NOT NULL BEGIN
      IF NOT EXISTS (SELECT DocID FROM #tblDomDocs WHERE DocID = @DocID) BEGIN
        RAISERROR('Error in sdom.spactDOMOpen: Invalid @DocID specified.', 16, 1)
      END      
    END
    ELSE BEGIN
      --Open a new DOM Document
     
      DECLARE @DocCount int
      IF @DocID IS NULL BEGIN
        SELECT
          @DocCount = COUNT(doc.DocID),
          @DocID = MIN(doc.DocID)
        FROM
          #tblDOMDocs doc
          
        IF @DocCount > 1 BEGIN
          RAISERROR('Error in sdom.spactDOMOpen:  @DocID was not specified, and there are multiple documents present in this session.', 16, 1)
        END
        ELSE IF @DocID IS NULL BEGIN
          INSERT INTO #tblDOMDocs (DateCreated)
          VALUES (GETDATE())
          
          SET @DocID = SCOPE_IDENTITY()
        END        
      END
    END
  END
END


GO
/*
**************************************************************************************
PROCEDURE sdom.spactDOMClear
Procedure sdom.spactDOMClear clears all data in the DOM
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spactDOMClear
@DocID int = NULL OUTPUT
AS BEGIN  
  SET NOCOUNT ON
    
  DELETE FROM #tblDOMAttribs WHERE DEID IN (SELECT DEID FROM #tblDOM WHERE @DocID IS NULL OR DocID = @DocID)
  DELETE FROM #tblDOMStyles WHERE DEID IN (SELECT DEID FROM #tblDOM WHERE @DocID IS NULL OR DocID = @DocID)
  DELETE FROM #tblDOM WHERE @DocID IS NULL OR DocID = @DocID
  
END

GO

/*
**************************************************************************************
PROCEUDRE sdom.spupdDOMAttribs
Procedure sdom.spupdDOMAttribs is to set Attributes of existing elements in the DOM
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spupdDOMAttribs
@DocID int = NULL OUTPUT,
@DEID int = NULL,
@ID varchar(512) = NULL,
@Name varchar(512) = NULL,
@Value varchar(MAX) = NULL,
@Attribs varchar(MAX) = NULL,
@Selector varchar(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON
  
  SET @Value = NULLIF(RTRIM(@Value), '')
  
  IF @DocID IS NULL EXEC sdom.spactDOMOpen @DocID = @DocID OUTPUT

  DECLARE @tvTargetList TABLE (
    DEID int PRIMARY KEY
  )
  
  IF @ID IS NOT NULL BEGIN
    SELECT @DEID = dom.DEID
    FROM
      #tblDOM dom 
    WHERE
      dom.DocID = @DocID AND
      dom.ID = @ID
  END
  ELSE IF @Selector IS NOT NULL BEGIN
    INSERT INTO @tvTargetList (DEID)
    EXEC sdom.spgetDOM @DocID = @DocID OUTPUT, @Selector = @Selector, @ReturnDEIDsOnly = 1
    SELECT TOP (1) @DEID = DEID FROM @tvTargetList
  END    
  
  WHILE @DEID IS NOT NULL BEGIN
    
    IF ISNULL(RTRIM(@Attribs), '') = '' BEGIN    
      DECLARE @TargetID int
      SELECT @TargetID = atr.DomAttribID
      FROM #tblDOMAttribs atr
      WHERE
         atr.DEID = @DEID AND 
         atr.Name = @Name 
               
      
      IF @TargetID IS NOT NULL BEGIN
        IF @Value IS NULL BEGIN
          DELETE FROM #tblDOMAttribs WHERE DOMAttribID = @TargetID
        END
        ELSE BEGIN
          UPDATE #tblDOMAttribs SET Value = @Value WHERE DOMAttribID = @TargetID
        END
      END
      ELSE BEGIN
        INSERT INTO #tblDOMAttribs (
          DEID,
          Name,
          Value)
        VALUES (
          @DEID,
          @Name,
          @Value
        )
      END
      
      --Assign special attributes
      UPDATE dom
      SET 
        ID = ISNULL(at_id.Value, dom.ID),     
        Name = ISNULL(at_name.Value, dom.Name),
        Class = ISNULL(at_class.Value, dom.Class)
      FROM
        #tblDOM dom
        LEFT JOIN #tblDOMAttribs at_id ON dom.DEID = at_id.DEID AND at_id.Name = 'id'
        LEFT JOIN #tblDOMAttribs at_name ON dom.DEID = at_name.DEID AND at_name.Name = 'name'      
        LEFT JOIN #tblDOMAttribs at_class ON dom.DEID = at_class.DEID AND at_class.Name = 'class'           
      WHERE
        dom.DocID = @DocID AND
        dom.DEID = @DEID
          
          
      DELETE FROM #tblDOMAttribs
      WHERE
        DEID = @DEID AND 
        Name in ('id', 'name', 'class')    
    END
    ELSE BEGIN            
      IF RTRIM(ISNULL(@Attribs, '')) <> '' BEGIN
        --Parse out attributes
        DECLARE @i int
        DECLARE @c char
        DECLARE @State varchar(40)
        DECLARE @InQuote bit
        DECLARE @NameStr varchar(MAX)
        DECLARE @ValueStr varchar(MAX)
        DECLARE @StartQuote char
        
        DECLARE @QuoteStartPos int
        DECLARE @QuoteEndPos int
        
        DECLARE @DoAttrib bit
       
        SET @InQuote = 0    

        SET @StartQuote = NULL    
        SET @State = 'AttribName'
        SET @i = 1

        SET @NameStr = ''
        SET @ValueStr = ''
            
        WHILE @i <= LEN(@Attribs) BEGIN
          SET @c = SUBSTRING(@Attribs, @i, 1)
                                  
          IF (@State = 'AttribValue') BEGIN
            IF (@c IN ('"', '''')) BEGIN
              IF (@InQuote = 0) AND ((@StartQuote IS NULL) OR (@c = @StartQuote)) BEGIN
                SET @InQuote = 1
                IF @StartQuote IS NULL BEGIN
                  SET @QuoteStartPos = @i
                  SET @StartQuote = @c
                END
              END
              ELSE IF (@InQuote = 1) AND (@c = @StartQuote) BEGIN
                SET @QuoteEndPos = @i
                SET @StartQuote = NULL
                SET @InQuote = 0
                IF (@i >= 2) AND (SUBSTRING(@Attribs, @i -1 , 1) = @c) BEGIN
                  SET @ValueStr = @ValueStr + @C
                END              
              END
              ELSE IF @c <> @StartQuote BEGIN
                SET @ValueStr = @ValueStr + @c         
              END
            END
            ELSE BEGIN
              SET @ValueStr = @ValueStr + @c
            END

            IF ((@c IN (' ', CHAR(9), CHAR(10), CHAR(13))) AND @InQuote = 0) OR (@i = LEN(@Attribs)) BEGIN
              SET @DoAttrib = 1
              SET @State = 'AttribName'
            END                     
          END
          
          ELSE BEGIN
            IF @State = 'AttribName' BEGIN
              IF @c = '=' BEGIN
                SET @State = 'AttribValue'
              END
              ELSE IF (@c IN (' ', CHAR(9), CHAR(10), CHAR(13))) BEGIN
                SET @DoAttrib = 1             
              END
              ELSE IF @i = LEN(@Attribs) BEGIN
                SET @NameStr = @NameStr + @c
                SET @DoAttrib = 1
              END               
              ELSE BEGIN
                IF @c NOT IN (' ', CHAR(9), CHAR(10), CHAR(13)) BEGIN
                  SET @NameStr = @NameStr + @c
                END
              END
            END  
          END       
          
          IF @DoAttrib = 1 BEGIN
            SET @DoAttrib = 0
            EXEC sdom.spupdDOMAttribs 
              @DocID = @DocID OUTPUT, 
              @DEID = @DEID, 
              @Name = @NameStr, 
              @Value = @ValueStr
            
            SET @NameStr = ''
            SET @ValueStr = ''
          END

          SET @i = @i + 1
        END                
            
      END    
    END
    
    DELETE FROM @tvTargetList WHERE DEID = @DEID

    SET @DEID = NULL
        
    IF EXISTS(SELECT DEID FROM @tvTargetList) BEGIN
      SELECT TOP (1) @DEID = DEID FROM @tvTargetList
    END
  END
END

GO

/*
**************************************************************************************
PROCEDURE sdom.spinsDOMNode
Procedure sdom.spinsDOMNode is to ADD elements to the DOM
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spinsDOMNode
@DocID int = NULL OUTPUT,
@Tag varchar(MAX),
@ID varchar(512) = NULL,
@Name varchar(512) = NULL,
@Class varchar(512) = NULL,
@Text varchar(MAX) = NULL,
@Attribs varchar(MAX) = NULL,
@OpenTagStartPos int = NULL,
@CloseTagEndPos int = NULL,
@ParentID varchar(512) = NULL,
@ParentDEID int = NULL,
@DEID int = NULL OUTPUT
AS
BEGIN  
  SET NOCOUNT ON
  
  /*
    Adds the specified node to the #tblDOM. If @Tag is specified, but @Text is not specified,
    a single normal node is added. If @Text is specified, then TWO nodes are added: one for
    the specified @Tag, and then a child text node.  (Text nodes have only the TextData and
    the ParentDEID:  they do not have tags or other attributes.)
    
    If @Tag is null, then only a text node is added.  It is added as a child of the parent that
    was specified.
    
    HTML comments are a special case.  For these the tag will be !-- and the comment node itself
    will store the comment body in TextData.  TextData will contain the start and end tags
    for the comment (such as <!-- Hello World -->).  There will not be a child text node.
    
    If @ParentID is specified, #tblDOM is searched for an existing node that has the
    specified HTML ID.  If found, the corresponding ParentDEID will be used as the parent
    for the new node.  Alternately, @ParentDEID may be spedified directly.  If both
    @ParentID and @ParentDEID are null, then the node will be added with a null parent--
    which indicates that it is a top level (or root level) node.

  */
  IF @DocID IS NULL EXEC sdom.spactDOMOpen @DocID = @DocID OUTPUT

  IF @ParentID IS NOT NULL BEGIN
    SELECT @ParentDEID = dom.DEID
    FROM
      #tblDOM dom
    WHERE
      dom.DocID = @DocID AND
      dom.ID = @ParentID    
  END
  
  SET @DEID = NULL
  
  IF (@Tag IS NOT NULL) BEGIN
    INSERT INTO #tblDOM (
      DocID,
      Tag,
      ID,
      Name,
      Class,
      TextData,
      OpenTagStartPos,
      CloseTagEndPos,
      ParentDEID)
    VALUES (
      @DocID,
      LOWER(@Tag),
      @ID,
      @Name,
      @Class,
      CASE WHEN (@Tag = '!--') THEN @Text ELSE NULL END,
      @OpenTagStartPos,
      @CloseTagEndPos,
      @ParentDEID)
      
    SET @DEID = SCOPE_IDENTITY() 
    SET @ParentDEID = @DEID
    
    --Store attributes  
    IF ISNULL(RTRIM(@Attribs), '') <> '' BEGIN
      EXEC sdom.spupdDOMAttribs
        @DocID = @DocID OUTPUT, 
        @DEID = @DEID, 
        @Attribs = @Attribs
    END    
    
  END
 
  IF (ISNULL(@Tag, '') <> '!--') AND (@Text IS NOT NULL) BEGIN     
    INSERT INTO #tblDOM (
      DocID,
      Tag,
      ID,
      Name,
      Class,
      TextData,
      ParentDEID)
    SELECT 
      @DocID,
      NULL AS Tag,
      NULL AS ID,
      NULL AS Name,
      NULL AS Class,
      @Text,
      @ParentDEID   
  END    
  
     
END

GO
-- Spi: 60 statements.

/*
**************************************************************************************
PROCEDURE sdom.spupdDOMStyles
Procedure sdom.spupdDOMStyles is to set Styles of existing elements in the DOM
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spupdDOMStyles
@DocID int = NULL,
@DEID int = NULL,
@ID varchar(512) = NULL,
@Name varchar(512),
@Value varchar(MAX)
AS
BEGIN
  SET NOCOUNT ON
  
  IF @DocID IS NULL EXEC sdom.spactDOMOpen @DocID = @DocID OUTPUT
  
  IF @ID IS NOT NULL BEGIN
    SELECT
      @DEID = dom.DEID
    FROM
      #tblDOM dom
    WHERE
      dom.DocID = @DocID AND
      dom.ID = @ID
  END
  ELSE BEGIN
    SELECT
      @DEID = dom.DEID
    FROM
      #tblDOM dom
    WHERE
      dom.DocID = @DocID AND
      dom.DEID = @DEID
  END
  
  DECLARE @TargetID int
  SELECT @TargetID = DOMStyleID FROM #tblDOMStyles WHERE DEID = @DEID AND Name = @Name
  
  IF @TargetID IS NOT NULL BEGIN
    IF @Value IS NULL BEGIN
      DELETE FROM #tblDOMStyles WHERE DOMStyleID = @TargetID
    END
    ELSE BEGIN
      UPDATE #tblDOMStyles SET Value = @Value 
      WHERE
        DOMStyleID = @TargetID
    END
  END
  ELSE BEGIN
    INSERT INTO #tblDOMStyles (
      DEID,
      Name,
      Value)
    VALUES (
      @DEID,
      @Name,
      @Value
    )
  END
END

GO


/*
*******************************************************************4*******************
PROCEDURE sdom.spgetDOM
Procedure sdom.spgetDOM is to retrive the internal DOM information as a resultset.
Provides JQuery-like functionality to select nodes from the DOM based on the
specified selector.  The selector can indicate #classes, .id's or tags.
If @Selector = NULL, the entire DOM will be returned.
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spgetDOM
@DocID int = NULL OUTPUT,
@Selector varchar(900) = NULL,
@ReturnDEIDsOnly bit = 0,
@SuppressResultset bit = 0
--$!ParseMarker
--Note:  comments and code between marker and AS are subject to automatic removal by OpsStream
--©Copyright 2006-2010 by David Rueter, Automated Operations, Inc.
--May be held, used or transmitted only pursuant to an in-force licensing agreement with Automated Operations, Inc.
--Contact info@opsstream.com / 800-964-3646 / 949-264-1555
AS 
BEGIN 
  SET NOCOUNT ON
  DELETE FROM #tblDOMHierarchy WHERE DocID = @DocID
  
  IF @DocID IS NULL EXEC sdom.spactDOMOpen @DocID = @DocID OUTPUT 

  IF @Selector IS NULL BEGIN
    --CTE Start -----------------------  
    ;WITH DOMTree (
      DEID,   
      DocID,   
      Tag,
      ID,
      Name,
      Class,
      TextData,
      OpenTagStartPos,
      CloseTagEndPos,
      ParentDEID,
      HUID,
      SortHUID,
      DOMLevel
    )
    AS 
    (
    SELECT
      dom.DEID,
      dom.DocID,    
      dom.Tag,
      dom.ID,
      dom.Name,
      dom.Class,
      dom.TextData,
      dom.OpenTagStartPos,
      dom.CloseTagEndPos,
      dom.ParentDEID,
      CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS HUID,
      CAST(RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
      1 AS DOMLevel   
    FROM
      #tblDOM dom
    WHERE
      dom.ParentDEID IS NULL

    UNION ALL

    SELECT
      dom.DEID,
      dom.DocID,    
      dom.Tag,
      dom.ID,
      dom.Name,
      dom.Class,
      dom.TextData,
      dom.OpenTagStartPos,
      dom.CloseTagEndPos,
      dom.ParentDEID,
      CAST(domch.HUID + '.' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS varchar(900)) AS HUID,
      CAST(domch.SortHUID + '.' + RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
      domch.DOMLevel + 1 
    FROM
      DOMTree domch
      JOIN #tblDOM dom ON
        domch.DEID = dom.ParentDEID        
    )    
    --CTE End -----------------------  
      
    INSERT INTO #tblDOMHierarchy       
    SELECT  
      dt.*,
      ROW_NUMBER() OVER (ORDER BY dt.SortHUID) AS Sequence,
      NULL AS HasChild
    FROM 
      DomTree dt
      JOIN #tblDOMDocs doc ON
        dt.DocID = doc.DocID      
    WHERE
      dt.DocID = @DocID 
    ORDER BY   
      dt.sortHUID   


    UPDATE r
    SET
      Sequence = r_seq.Sequence
    FROM 
      #tblDOMHierarchy r
      JOIN (
        SELECT
          r.DEID,
          ROW_NUMBER() OVER (ORDER BY r.SortHUID) AS Sequence
        FROM
          #tblDOMHierarchy r
        ) r_seq ON
      r.DEID = r_seq.DEID
      
    UPDATE r
    SET
      HasChild = CASE WHEN r2.DOMLevel > r.DOMLevel THEN 1 ELSE 0 END
    FROM
      #tblDOMHierarchy r
      JOIN #tblDOMHierarchy r2 ON
        r.Sequence + 1 = r2.Sequence
      
           
    IF @SuppressResultset = 0 BEGIN
      SELECT * FROM #tblDOMHierarchy
    END
  END
  ELSE BEGIN

    SET @Selector = RTRIM(@Selector) + ' '
    
    DECLARE @c char
    DECLARE @i int
    
    DECLARE @Mode varchar(40)
    DECLARE @SelWhere varchar(MAX)
    DECLARE @SelTerm varchar(MAX)
    
    
    --default selector is Tag
    SET @Mode = 'tag'  

    SET @i = 1
    WHILE @i <= LEN(@Selector) BEGIN
    
      SET @c = SUBSTRING(@Selector, @i, 1)
      
      IF @c IN ('.', '#', ' ') BEGIN
        IF @c = '.' BEGIN
          SET @Mode = 'id'
        END
        ELSE IF @c = '#' BEGIN
          SET @Mode = 'class'
        END
        ELSE IF @C = ' ' BEGIN
          --apply selector  
          SET @SelWhere = ISNULL(@SelWhere + ' AND ', '') + @SelTerm   
        END
        SET @SelTerm = NULL 
      END
      ELSE BEGIN
        SET @SelTerm = ISNULL(@SelTerm, '') + @c
      END   
      
      SET @i = @i + 1    
    END

    IF @ReturnDEIDsOnly = 1 BEGIN
      IF @Mode = 'class' BEGIN
        SELECT DEID
        FROM
          #tblDOM dom
        WHERE
          dom.DocID = @DocID AND
          dom.Class = @SelTerm
      END
      ELSE IF @Mode = 'id' BEGIN
        SELECT DEID
        FROM
          #tblDOM dom       
        WHERE
          dom.DocID = @DocID AND      
          dom.ID = @SelTerm
      END
      ELSE IF @Mode = 'tag' BEGIN
        SELECT DEID
        FROM
          #tblDOM dom         
        WHERE
          dom.DocID = @DocID AND       
          dom.Tag = @SelTerm
      END  
    END
    ELSE BEGIN
      IF @Mode = 'class' BEGIN
        --CTE Start -----------------------  
        ;WITH DOMTree (
          DEID,   
          DocID,   
          Tag,
          ID,
          Name,
          Class,
          TextData,
          OpenTagStartPos,
          CloseTagEndPos,
          ParentDEID,
          HUID,
          SortHUID,
          DOMLevel
        )
        AS 
        (
        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS HUID,
          CAST(RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          1 AS DOMLevel   
        FROM
          #tblDOM dom
        WHERE
          dom.ParentDEID IS NULL

        UNION ALL

        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(domch.HUID + '.' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS varchar(900)) AS HUID,
          CAST(domch.SortHUID + '.' + RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          domch.DOMLevel + 1 
        FROM
          DOMTree domch
          JOIN #tblDOM dom ON
            domch.DEID = dom.ParentDEID        
        )    
        --CTE End -----------------------                             
      
        INSERT INTO #tblDOMHierarchy
        SELECT  
          dt.*,
          ROW_NUMBER() OVER (ORDER BY dt.SortHUID) AS Sequence,
          NULL AS HasChild
        FROM 
          DomTree dt
          JOIN #tblDOMDocs doc ON
            dt.DocID = doc.DocID      
        WHERE
          dt.DocID = @DocID AND         
          dt.Class = @SelTerm
        ORDER BY   
          dt.sortHUID
              
        IF @SuppressResultset = 0 BEGIN
          SELECT * FROM #tblDOMHierarchy
        END
      END
      ELSE IF @Mode = 'id' BEGIN
        --CTE Start -----------------------  
        ;WITH DOMTree (
          DEID,   
          DocID,   
          Tag,
          ID,
          Name,
          Class,
          TextData,
          OpenTagStartPos,
          CloseTagEndPos,
          ParentDEID,
          HUID,
          SortHUID,
          DOMLevel
        )
        AS 
        (
        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS HUID,
          CAST(RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          1 AS DOMLevel   
        FROM
          #tblDOM dom
        WHERE
          dom.ParentDEID IS NULL

        UNION ALL

        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(domch.HUID + '.' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS varchar(900)) AS HUID,
          CAST(domch.SortHUID + '.' + RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          domch.DOMLevel + 1 
        FROM
          DOMTree domch
          JOIN #tblDOM dom ON
            domch.DEID = dom.ParentDEID        
        )    
        --CTE End -----------------------    
      
        INSERT INTO #tblDOMHierarchy
        SELECT  
          dt.*,
          ROW_NUMBER() OVER (ORDER BY dt.SortHUID) AS Sequence,
          NULL AS HasChild
        FROM 
          DomTree dt
          JOIN #tblDOMDocs doc ON
            dt.DocID = doc.DocID      
        WHERE
          dt.DocID = @DocID AND         
          dt.Class = @SelTerm
        ORDER BY   
          dt.sortHUID
          
        IF @SuppressResultset = 0 BEGIN
          SELECT * FROM #tblDOMHierarchy
        END 
      END
      ELSE IF @Mode = 'tag' BEGIN
        --CTE Start -----------------------  
        ;WITH DOMTree (
          DEID,   
          DocID,   
          Tag,
          ID,
          Name,
          Class,
          TextData,
          OpenTagStartPos,
          CloseTagEndPos,
          ParentDEID,
          HUID,
          SortHUID,
          DOMLevel
        )
        AS 
        (
        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS HUID,
          CAST(RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          1 AS DOMLevel   
        FROM
          #tblDOM dom
        WHERE
          dom.ParentDEID IS NULL

        UNION ALL

        SELECT
          dom.DEID,
          dom.DocID,    
          dom.Tag,
          dom.ID,
          dom.Name,
          dom.Class,
          dom.TextData,
          dom.OpenTagStartPos,
          dom.CloseTagEndPos,
          dom.ParentDEID,
          CAST(domch.HUID + '.' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)) AS varchar(900)) AS HUID,
          CAST(domch.SortHUID + '.' + RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY dom.DEID) AS varchar(900)), 6) AS varchar(900)) AS SortHUID,
          domch.DOMLevel + 1 
        FROM
          DOMTree domch
          JOIN #tblDOM dom ON
            domch.DEID = dom.ParentDEID        
        )    
        --CTE End -----------------------                
      
        INSERT INTO #tblDOMHierarchy
        SELECT  
          dt.*,
          ROW_NUMBER() OVER (ORDER BY dt.SortHUID) AS Sequence,
          NULL AS HasChild
        FROM 
          DomTree dt
          JOIN #tblDOMDocs doc ON
            dt.DocID = doc.DocID      
        WHERE
          dt.DocID = @DocID AND         
          dt.Class = @SelTerm
        ORDER BY   
          dt.sortHUID
   

        UPDATE r
        SET
          Sequence = r_seq.Sequence
        FROM 
          #tblDOMHierarchy r
          JOIN (
            SELECT
              r.DEID,
              ROW_NUMBER() OVER (ORDER BY r.SortHUID) AS Sequence
            FROM
              #tblDOMHierarchy r
            ) r_seq ON
          r.DEID = r_seq.DEID
          
        UPDATE r
        SET
          HasChild = CASE WHEN r2.DOMLevel > r.DOMLevel THEN 1 ELSE 0 END
        FROM
          #tblDOMHierarchy r
          JOIN #tblDOMHierarchy r2 ON
            r.Sequence + 1 = r2.Sequence
         
          
        IF @SuppressResultset = 0 BEGIN
          SELECT * FROM #tblDOMHierarchy
        END 
      END
    END  
  END

END

GO


/*
**************************************************************************************
PROCEDURE sdom.spgetDOMHTML
Procedure sdom.spgetDOMHTML is to render an HTML string based on the internal data in
the DOM
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spgetDOMHTML
@DocID int = NULL OUTPUT,
@ForceDocType varchar(MAX) = NULL,
@PrettyWhitespace bit = 0,
@HTML varchar(MAX) = NULL OUTPUT,
@PrintHTML bit = 1,
@HUIDLike varchar(900) = NULL
--$!ParseMarker
--Note:  comments and code between marker and AS are subject to automatic removal by OpsStream
--©Copyright 2006-2010 by David Rueter, Automated Operations, Inc.
--May be held, used or transmitted only pursuant to an in-force licensing agreement with Automated Operations, Inc.
--Contact info@opsstream.com / 800-964-3646 / 949-264-1555
AS 
BEGIN   
  SET NOCOUNT ON
  
  DECLARE @Debug bit
  SET @Debug = 0
  
  IF @DocID IS NULL EXEC sdom.spactDOMOpen @DocID = @DocID OUTPUT
  

  --local table to hold stack of tags
  DECLARE @tvTagStack TABLE (
    StackID int identity PRIMARY KEY, --facilitates deletes
    DEID int,
    CloseTag varchar(900),
    IncludeNode bit
  );

  DECLARE @CRLF varchar(5)
  SET @CRLF = CHAR(13) + CHAR(10)

  EXEC sdom.spgetDOM @DocID = @DocID OUTPUT, @SuppressResultset = 1
  
  UPDATE r
  SET
    Sequence = r_seq.Sequence
  FROM 
    #tblDOMHierarchy r
    JOIN (
      SELECT
        r.DEID,
        ROW_NUMBER() OVER (ORDER BY r.SortHUID) AS Sequence
      FROM
        #tblDOMHierarchy r
      ) r_seq ON
    r.DEID = r_seq.DEID
    
  UPDATE r
  SET
    HasChild = CASE WHEN r2.DOMLevel > r.DOMLevel THEN 1 ELSE 0 END
  FROM
    #tblDOMHierarchy r
    JOIN #tblDOMHierarchy r2 ON
      r.Sequence + 1 = r2.Sequence
                  
     
  DECLARE curDOM CURSOR LOCAL STATIC FOR 
  SELECT
    r.DEID,
    r.Tag,
    r.ID,
    r.Name,
    r.Class,
    r.TextData,
    r.ParentDEID,
    r.HUID,
    r.DOMLevel,
    r.HasChild,
    CAST(CASE WHEN @HUIDLike IS NULL OR r.HUID LIKE @HUIDLike + '%' THEN 1 ELSE 0 END AS bit) AS IncludeNode
  FROM
    #tblDOMHierarchy r
  ORDER BY
    r.Sequence
  
  DECLARE @DEID int
  DECLARE @Tag varchar(MAX)
  DECLARE @ID varchar(512)
  DECLARE @Name varchar(512)
  DECLARE @Class varchar(512)
  DECLARE @TextData varchar(MAX)
  DECLARE @ParentDEID int
  DECLARE @HUID varchar(900)
  DECLARE @DOMLevel int
  DECLARE @HasChild bit
  DECLARE @IncludeNode bit
   
 
  DECLARE @RenderedHTML varchar(MAX)  
  
  DECLARE @DonePop bit
  DECLARE @AllowPush bit 
  
  DECLARE @StackID int  
  DECLARE @StackDEID int
  DECLARE @StackTag varchar(MAX)  
  DECLARE @StackIncludeNode bit
     
  DECLARE @EmitTag varchar(MAX)
  
  DECLARE @ThisStyle varchar(MAX)
  
  DECLARE @ThisAttribID int
  DECLARE @LastAttribID int
  DECLARE @ThisAttribName varchar(MAX)
  DECLARE @ThisAttribValue varchar(MAX)

  DECLARE @CurParentDEID int
  DECLARE @CurParentTag varchar(MAX)
  DECLARE @CurIncludeNode bit
 
  OPEN curDOM
  FETCH curDOM INTO
    @DEID,
    @Tag,
    @ID,
    @Name,
    @Class,
    @TextData,
    @ParentDEID,
    @HUID,
    @DOMLevel,
    @HasChild,
    @IncludeNode
        
  SET @RenderedHTML = NULL
  SET @CurParentDEID = NULL
  SET @CurParentTag = NULL
  SET @CurIncludeNode = NULL
  SET @DonePop = NULL 
        
  WHILE @@FETCH_STATUS = 0 BEGIN
    --Walk through each node of the DOM to render HTML
    SET @ThisStyle = NULL
    SET @ThisAttribID = NULL
    SET @LastAttribID = NULL
    
    SET @EmitTag = NULL
    SET @AllowPush = NULL
        
    SET @StackID = NULL    
    SET @StackDEID = NULL
    SET @StackTag = NULL   
    SET @StackIncludeNode = NULL
    
    IF @DonePop IS NULL BEGIN
      --first pass through
      SET @CurParentDEID = @ParentDEID
      SET @CurIncludeNode = @IncludeNode
      SET @CurParentTag = '</' + @Tag + '>'
      SET @DonePop = 1
    END
    
    IF @Debug = 1 PRINT 'Starting node @Tag = ' + ISNULL(@Tag, 'NULL') + 
      ' @DEID = ' + ISNULL(CAST(@DEID AS varchar(100)), 'NULL') + 
      ' @ParentDEID = ' + ISNULL(CAST(@ParentDEID AS varchar(100)), 'NULL') +
      ' @CurParentDEID = ' + ISNULL(CAST(@CurParentDEID AS varchar(100)), 'NULL')
   
    --#1:  See if there is anything we need to pop.  Close tags, set CurParent as needed.
    IF ISNULL(@ParentDEID, 0) <> ISNULL(@CurParentDEID, 0) --AND
   --   (@Tag IS NOT NULL) AND (@Tag NOT LIKE '!%') 
    BEGIN
      
      IF @Debug = 1 IF @Debug = 1 PRINT 'TRACE: Need to pop'
      
      --need to pop         
      SET @DonePop = 0
      WHILE @DonePop = 0 BEGIN
             
        SET @StackID = NULL      
        SET @StackDEID = NULL    
        SET @StackTag = NULL
        SET @StackIncludeNode = NULL
        
        SELECT TOP (1) 
          @StackID = StackID,          
          @StackDEID = DEID,              
          @StackTag = CloseTag,
          @StackIncludeNode = IncludeNode
        FROM
          @tvTagStack
        ORDER BY
          StackID DESC  
        
        SET @DonePop = 1
        
        IF @Debug = 1 IF @Debug = 1 PRINT 'TRACE: Popped from @CurParentDEID = ' +
          ISNULL(CAST(@CurParentDEID AS varchar(100)), 'NULL') + ' to ' +
          ISNULL(CAST(@StackDEID AS varchar(100)), 'NULL')
        
        IF (@CurIncludeNode = 1) AND (@CurParentTag IS NOT NULL) BEGIN
          SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + @CurParentTag
        END
        
        SET @CurParentDEID = @StackDEID
        SET @CurParentTag = @StackTag
        SET @CurIncludeNode = @StackIncludeNode
        
        --Note:  CurParent is left open.  May be re-pushed
                    
        IF @StackID IS NULL BEGIN
          SET @DonePop = 1
        END
        ELSE BEGIN
          DELETE FROM @tvTagStack WHERE StackID = @StackID
          
          IF ISNULL(@ParentDEID, 0) <> ISNULL(@CurParentDEID, 0) BEGIN
            SET @DonePop = 0
            --render close tag
--            SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + @StackTag
          END
          
        END
                   
      END --WHILE @DonePop = 0
    END  --IF CurParent change needed
    
    
    IF ISNULL(@ParentDEID, 0) <> ISNULL(@CurParentDEID, 0) BEGIN
      PRINT 'Error in DOM:  could not pop back to where @ParentDEID = @CurParentDEID ' +
      '(@ParentDEID = ' + ISNULL(CAST(@ParentDEID AS varchar(100)), 'NULL') +
      ' @CurParentDEID = ' + ISNULL(CAST(@CurParentDEID AS varchar(100)), 'NULL') + ')'
    END

      
    --#2: Render tag
    IF (@IncludeNode = 1) AND (@Tag = '!--') BEGIN
      --HTML Comment
      SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + ISNULL(@TextData, '')
      SET @AllowPush = 0
    END
    ELSE IF (@IncludeNode = 1) AND (@Tag LIKE '!%') BEGIN
      --declaration
      SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + ISNULL('<' + @TextData + '>', '')
      SET @AllowPush = 0      
    END
    ELSE IF (@IncludeNode = 1) AND (@Tag IS NULL) BEGIN
      --text node
      SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + ISNULL(@TextData, '')
      SET @AllowPush = 0      
    END      
    ELSE BEGIN     
      --normal node
      SET @AllowPush = 1      
      
      SET @EmitTag = '<' + @Tag + 
        ISNULL(' id="' + @ID + '"', '') + 
        ISNULL(' name="' + @Name + '"', '') + 
        ISNULL(' class="' + @Class + '"', '')                                          
                 
      IF EXISTS (SELECT DOMStyleID FROM #tblDOMStyles WHERE DEID = @DEID) BEGIN
        SET @ThisAttribID = -1
        WHILE @ThisAttribID IS NOT NULL BEGIN 
          SET @ThisAttribID = NULL      
          SELECT TOP (1)
            @ThisAttribID = da.DOMStyleID,
            @ThisAttribName = da.Name,
            @ThisAttribValue = da.Value
          FROM
            #DOMStyles da
          WHERE
            da.DEID = @DEID AND
            da.DOMStyleID > ISNULL(@LastAttribID, 0)
          ORDER BY
            da.DOMStyleID
        
          IF @ThisAttribID IS NOT NULL BEGIN
            SET @ThisStyle = ISNULL(@ThisStyle, '') + ISNULL(@ThisAttribName + ': ' + @ThisAttribValue + ';', '') 
          END
          
          SET @LastAttribID = @ThisAttribID        
        END
      END
      
      --save list of styles in style attribute
      EXEC sdom.spupdDOMAttribs @DocID = @DocID OUTPUT, @DEID = @DEID, @Name = 'style', @Value = @ThisStyle
            
      IF EXISTS (SELECT DOMAttribID FROM #tblDOMAttribs WHERE DEID = @DEID) BEGIN
        SET @ThisAttribID = -1
        WHILE @ThisAttribID IS NOT NULL BEGIN 
          SET @ThisAttribID = NULL      
          SELECT TOP (1)
            @ThisAttribID = da.DOMAttribID,
            @ThisAttribName = da.Name,
            @ThisAttribValue = da.Value
          FROM
            #tblDOMAttribs da
          WHERE
            da.DEID = @DEID AND
            da.DOMAttribID > ISNULL(@LastAttribID, 0)
          ORDER BY
            da.DOMAttribID
        
          IF @ThisAttribID IS NOT NULL BEGIN
            SET @EmitTag = @EmitTag + ISNULL(' ' + @ThisAttribName + '="' + @ThisAttribValue + '"', '') 
          END
          
          SET @LastAttribID = @ThisAttribID        
        END
      END    
      
      IF @IncludeNode = 1 BEGIN           
        SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + @EmitTag +
          CASE WHEN @HasChild = 0 THEN '/' ELSE '' END + '>' 
      END
    END
    
        
    --#3: Set CurParentDEID = new node, if applicable
    IF (@AllowPush = 1) AND (@HasChild = 1) BEGIN
      --push and move CurParent to newly-inserted node
          
      IF @CurParentDEID IS NOT NULL BEGIN         
        INSERT INTO @tvTagStack (
          DEID,     
          CloseTag,
          IncludeNode
          
        )
        VALUES (    
          @CurParentDEID, 
          @CurParentTag,
          @CurIncludeNode
       )   
      END
     
      IF @Debug = 1 IF @Debug = 1 PRINT 'TRACE: Push @CurParentDEID = ' + ISNULL(CAST(@CurParentDEID AS varchar(100)), 'NULL') + 
       ' New @CurParentDEID = ' +  ISNULL(CAST(@DEID AS varchar(100)), 'NULL') 
       
      SET @CurParentDEID = @DEID
      SET @CurIncludeNode = @IncludeNode
      SET @CurParentTag =  '</' + @Tag + '>'                
         
    END      
      
                       
    FETCH curDOM INTO
      @DEID,
      @Tag,
      @ID,
      @Name,
      @Class,
      @TextData,
      @ParentDEID,
      @HUID,
      @DOMLevel,
      @HasChild,
      @IncludeNode
  END
  CLOSE curDOM 

  IF (@CurIncludeNode = 1) AND (@CurParentTag IS NOT NULL) BEGIN
    SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + @CurParentTag
  END

  WHILE EXISTS(SELECT StackID FROM @tvTagStack) BEGIN  
  
    SELECT TOP (1) 
      @StackID = StackID,          
      @StackDEID = DEID,              
      @StackTag = CloseTag,
      @StackIncludeNode = IncludeNode
    FROM
      @tvTagStack
    ORDER BY
      StackID DESC               
              
    DELETE FROM @tvTagStack WHERE StackID = @StackID
     
    IF @StackIncludeNode = 1 BEGIN                             
      SET @RenderedHTML = ISNULL(@RenderedHTML + CASE WHEN @PrettyWhitespace = 1 THEN @CRLF ELSE '' END, '') + ISNULL(@StackTag, '')                               
    END
  END

  SET @HTML = @RenderedHTML
  
  IF @PrintHTML = 1 BEGIN
    PRINT @HTML
  END
END

GO


/*
**************************************************************************************
PROCEDURE sdom.spactDOMLoad
Procedure sdom.spactDOMLoad parses the provided @HTML and loads into DOM.

If @ID or @DEID is specified, modifes existing DOM starting with the specified node.

If @Selector is specified, the #Load operation will be performed for each node
that matches the specified selection.

If @ReplaceOuter = 1 the specified node itself will also be replaced (i.e. OUTER HTML),
otherwise only the children of the specified node will be replaced (i.e. INNER
HTML)

If neither @ID or @DEID is specified, clears entire DOM and loads from @HTML.

@Attribs may specify a string of Attributes that will be appended to every node
affected by sdom.spactDOMLoad.

IF @Class is specifed, 
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spactDOMLoad
@DocID int = NULL,
@HTML varchar(MAX),
@ID varchar(512) = NULL,
@DEID int = NULL,
@ReplaceOuter bit = 0,
@CreateNew bit = 0,

@Selector varchar(MAX) = NULL,
@IncludeAllWhitespace bit = 0,
@Tolerate bit = 0
AS
BEGIN
  SET NOCOUNT ON
  
  EXEC sdom.spactDOMOpen @CreateNew = @CreateNew, @DocID = @DocID OUTPUT


  --local table to hold stack of tags
  DECLARE @tvTagStack TABLE (
    TagStackID int identity PRIMARY KEY ,
    Tag varchar(512),
    DEID int,
    ParentDEID int
  );
  
  DECLARE @tvTargetList TABLE (
    DEID int PRIMARY KEY
  ) 
   
  
  DECLARE @TargetDEID int 
  
  IF @ID IS NOT NULL BEGIN
    SELECT
      @DEID = dom.DEID
    FROM
      #tblDOM dom
    WHERE
      dom.DocID = @DocID AND
      dom.ID = @ID
  END
  ELSE IF @Selector IS NOT NULL BEGIN
    INSERT INTO @tvTargetList (DEID)
    EXEC sdom.spgetDOM @DocID = @DocID OUTPUT, @Selector = @Selector, @ReturnDEIDsOnly = 1
    SELECT TOP (1) @DEID = DEID FROM @tvTargetList
  END
   
   
  DECLARE @i int
  DECLARE @c char

  DECLARE @IsSingleton bit
  DECLARE @InComment bit
  DECLARE @InQuote bit
  DECLARE @StartQuote char
  
  DECLARE @QuoteStartPos int
  DECLARE @QuoteEndPos int

  DECLARE @ParentDEID int
  DECLARE @LastDEID int
  
  DECLARE @TopStackID int
  DECLARE @StackTag varchar(8000)
  DECLARE @PopDone bit

  DECLARE @State varchar(40)
  DECLARE @OpenTagName varchar(512)
  DECLARE @CloseTagName varchar(512)
  
  DECLARE @Text varchar(MAX)
  DECLARE @AttribStr varchar(MAX)
  DECLARE @CommentStr varchar(MAX)
 
  DECLARE @TextChunk varchar(8000)
  DECLARE @AttribChunk varchar(8000)  
  
  DECLARE @StartPos int
  DECLARE @EndPos int
  DECLARE @CommentStartPos int
  DECLARE @TextLen int
  DECLARE @TextChunkLen int

  DECLARE @DoOpenTag bit
  DECLARE @DoCloseTag bit
  DECLARE @ImmediateClose bit
 
   
  IF (@DEID IS NULL) AND (@Selector IS NULL) BEGIN
    EXEC sdom.spactDOMClear @DocID = @DocID OUTPUT
    SET @DEID = -1
  END
    
  WHILE @DEID IS NOT NULL BEGIN
    SET @Text = ''
    SET @CommentStr = ''
    SET @LastDEID = NULL
    SET @ParentDEID = NULL 
    SET @OpenTagName = NULL
    SET @CloseTagName = NULL
    
    SET @ImmediateClose = 0
    SET @IsSingleton = 0
    SET @InComment = 0
    SET @InQuote = 0
    SET @StartQuote = NULL
    
    SET @StartPos = NULL
    SET @EndPos = NULL
    SET @CommentStartPos = NULL

    SET @TextChunk = ''
    SET @AttribChunk = ''
    
    SET @Text = ''
    SET @AttribStr = ''
    SET @CommentStr = ''
      
    SELECT 
      @ParentDEID = CASE WHEN @ReplaceOuter = 1 THEN dom.ParentDEID ELSE dom.DEID END
    FROM
      #tblDOM dom
    WHERE
      dom.DocID = @DocID AND
      dom.DEID = @DEID            
                   
    --Note:  we are replacing all child nodes.  We might be replacing 
    --the target node too--if @ReplaceOuter = 1
       
    IF @HTML IS NOT NULL BEGIN  
      DELETE FROM #tblDOM
      WHERE
        (((@ReplaceOuter = 1 ) AND (DEID = @DEID)) OR
         ((ParentDEID = @DEID) AND (LEFT(@HTML, 1) = '<')))    
      
      SET @i = 1
      
      SET @OpenTagName = ''
      SET @CloseTagName = ''
        
      SET @State = 'Text'
               
      WHILE @i <= LEN(@HTML) BEGIN        
        SET @c = SUBSTRING(@HTML, @i, 1)      
          
        --IF @State = 'Comment' BEGIN
        IF @InComment = 1 BEGIN
          --special case:  locked in processing text until -->                        
          SET @CommentStr = @CommentStr + @c
                           
          IF PATINDEX('%-->%', @CommentStr) > 0 BEGIN
            --reached the end of the comment            
            EXEC sdom.spinsDOMNode 
              @DocID = @DocID OUTPUT,
              @Tag = '!--',
              @Text = @CommentStr, 
              @OpenTagStartPos = @CommentStartPos,
              @CloseTagEndPos = @i,
              @ParentDEID = @ParentDEID
              
            SET @CommentStr = ''
            SET @CommentStartPos = 0
                      
            SET @State = 'Text'
            SET @InComment = 0
            --SET @i = @i + 1            
          END          
        END  
        ELSE BEGIN
          IF (@i = LEN(@HTML)) AND 
             ((@IncludeAllWhitespace = 1) OR
              (@C NOT IN (CHAR(9), CHAR(10), CHAR(13), ' '))) BEGIN
              
            --at the last character of our @HTML
            IF @IncludeAllWhitespace = 1 BEGIN
              SET @TextLen = LEN(@Text + 'x') - 1
              SET @TextChunkLen = LEN(@TextChunk + 'x') - 1           
            END
            ELSE BEGIN          
              EXEC sdom.spgetLenNTW @s = @Text, @Len = @TextLen OUTPUT
              EXEC sdom.spgetLenNTW @s = @TextChunk, @Len = @TextChunkLen OUTPUT   
            END       
          
            IF (@TextLen > 0) OR (@TextChunkLen > 0) BEGIN        
              --special case of text-only @HTML (no tags)
              SET @TextChunk = @TextChunk + @c  
              IF @TextChunk <> '' BEGIN
                SET @Text = @Text + @TextChunk          
                SET @TextChunk = ''
              END
      
              EXEC sdom.spinsDOMNode
                @DocID = @DocID OUTPUT,
                @Tag = NULL,
                @Text = @Text,
                @ParentDEID = @ParentDEID,
                @DEID = @LastDEID OUTPUT   
                
              SET @Text = ''                      
            END
          END        
           
          --special occurrences of / Note that these could have been coded to
          --be handled below in each respective State, but seemed more clear to
          --keep together here.
          ELSE IF (@c = '/') AND (@State = 'StartTag') BEGIN
            SET @State = 'CloseTagName'
          END
          ELSE IF (@c = '/') AND (@State = 'OpenTagName') BEGIN  
            --Immediate close of tag.  Actual close will happen on >
            SET @ImmediateClose = 1        
          END
          ELSE IF (@c = '/') AND (@State = 'CloseTagName') BEGIN  
            --NOOP:  we want to drop the /
            SET @c = @c
          END        
          ELSE IF (@c = '/') AND (@State = 'Attributes') AND (@InQuote = 0) BEGIN
            IF @Tolerate = 1 BEGIN
              IF SUBSTRING(@HTML, @i + 1, 1) <> '>' BEGIN
                --False alarm:  HTML is missing quotes around attribute values.
                --This is not really an indication of the end of the tag.

                SET @AttribChunk = @AttribChunk + @c
                IF LEN(@AttribChunk) = 8000 BEGIN
                  SET @AttribStr = @AttribStr + @AttribChunk
                  SET @AttribChunk = ''
                END   
              END
              ELSE BEGIN
                SET @State = 'OpenTagName'
                SET @ImmediateClose = 1                           
              END
            END
            ELSE BEGIN
              SET @State = 'OpenTagName'
              SET @ImmediateClose = 1                           
            END          
            
          END
          
          
          ELSE IF (@c = '<') BEGIN
            SET @StartPos = @i

            IF @TextChunk <> '' BEGIN
             SET @Text = @Text + @TextChunk          
              SET @TextChunk = ''
            END        

            IF @Text  <> '' BEGIN            
              --reached the end of the text node
              EXEC sdom.spgetLenNTW @s = @Text, @Len = @TextLen OUTPUT
              
              IF ((@IncludeAllWhitespace = 1) OR (@TextLen > 0)) BEGIN             
                EXEC sdom.spinsDOMNode
                  @DocID = @DocID OUTPUT,
                  @Tag = NULL,
                  @Text = @Text,
                  @ParentDEID = @ParentDEID,
                  @DEID = @LastDEID OUTPUT 
                  
                SET @Text = ''             
              END                 
            END

            --See if we are starting a comment
            IF SUBSTRING(@HTML, @i, LEN('<!--')) = '<!--' BEGIN
              --SET @State = 'Comment'
              SET @InComment = 1
              SET @CommentStr = @c
              SET @CommentStartPos = @i
            END
            ELSE BEGIN
              --otherwise we are just starting a new tag          
              SET @State = 'StartTag'
            END
          END
          
          ELSE IF (@c = '>') BEGIN  
            IF @State = 'CloseTagName' BEGIN
              SET @EndPos = @i
              SET @IsSingleton = CASE WHEN 
                (@CloseTagName IN ('area', 'br', 'col', 'command', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'param', 'source')) OR
                (@CloseTagName LIKE '!%') THEN 1 ELSE 0 END
                
              IF @IsSingleton = 0 BEGIN
                --Not a singleton HTML tag for which we ignore the close tag if present         
                SET @DoCloseTag = 1
              END
            END
            ELSE IF @State IN ('OpenTagName', 'Attributes') BEGIN
              SET @IsSingleton = CASE WHEN
               (@OpenTagName IN ('area', 'br', 'col', 'command', 'embed', 'hr', 'img', 'input', 'link', 'meta', 'param', 'source')) OR
               (@OpenTagName LIKE '!%') THEN 1 ELSE 0 END
               
              IF @IsSingleton = 1 BEGIN
                --Singleton HTML tag that does not need to be closed
                SET @ImmediateClose = 1
              END
              SET @DoOpenTag = 1  
            END
          END
          
          ELSE IF @State = 'StartTag' BEGIN
            --not a / because that case was handled above
            SET @State = 'OpenTagName'
            SET @OpenTagName = @c
          END
          ELSE IF @State = 'OpenTagName' BEGIN
            IF @c IN (' ', CHAR(9), CHAR(10), CHAR(13)) BEGIN
              SET @State = 'Attributes'
            END
            ELSE BEGIN
              --Not a / because that case was handled above
              SET @OpenTagName = @OpenTagName + @c
            END
          END
          ELSE IF @State = 'CloseTagName' BEGIN
            SET @CloseTagName = @CloseTagName + @c
          END
          ELSE IF @State = 'Attributes' BEGIN
            --not a / because that case was handled above
            IF (@c IN ('"', '''')) BEGIN
              IF (@InQuote = 0) AND ((@StartQuote IS NULL) OR (@c = @StartQuote)) BEGIN
                SET @InQuote = 1
                IF @StartQuote IS NULL BEGIN
                  SET @QuoteStartPos = @i
                  SET @StartQuote = @c
                END
              END
              ELSE IF (@InQuote = 1) AND (@c = @StartQuote) BEGIN
                SET @QuoteEndPos = @i
                SET @InQuote = 0
                SET @StartQuote = NULL                
              END
            END

            SET @AttribChunk = @AttribChunk + @c
            IF LEN(@AttribChunk) = 8000 BEGIN
              SET @AttribStr = @AttribStr + @AttribChunk
              SET @AttribChunk = ''
            END          
   
          END
          
          ELSE IF @State IN ('Text') BEGIN


            SET @TextChunk = @TextChunk + @c              



            IF LEN(@TextChunk) = 8000 BEGIN
             SET @Text = @Text + @TextChunk          
              SET @TextChunk = ''
            END    
                     
          END      
          ELSE BEGIN
            RAISERROR('Error in #Load:  Unexpected state parsing HTML', 16, 1)
          END
                                   
          --Processing for completed OpenTag
          IF @DoOpenTag = 1 BEGIN
           
            SET @DoOpenTag = 0  

            IF @AttribChunk <> '' BEGIN
              SET @AttribStr = @AttribStr + @AttribChunk
              SET @AttribChunk = ''
            END
            
            IF @ImmediateClose = 1 BEGIN
              SET @EndPos = @i --should be called on the >          
            END
            
            IF @OpenTagName = 'script' BEGIN
              --A special case:  we know that there must be an end tag for the script
              --(required in all cases), and we know we don't want to inspect the contents
              --of the script block.  So we can copy the whole block at once here and
              --save some looping and concatenating.
              SET @Text = RIGHT(@HTML, LEN(@HTML + 'x') - 1 - @i)
              SET @Text = LEFT(@Text, PATINDEX('%</script>%', @Text) - 1)                                              
              SET @i = @i + LEN(@Text + 'x') - 1 + LEN('</script>') 
              SET @EndPos = @i  
              
              EXEC sdom.spinsDOMNode 
                @DocID = @DocID OUTPUT,
                @Tag = @OpenTagName,
                @Attribs = @AttribStr, 
                @Text = @Text,
                @OpenTagStartPos = @StartPos,
                @CloseTagEndPos = @EndPos, 
                @ParentDEID = @ParentDEID,
                @DEID = @LastDEID OUTPUT      
                
                SET @Text = ''     
            END
            ELSE BEGIN          
              EXEC sdom.spinsDOMNode 
                @DocID = @DocID OUTPUT,
                @Tag = @OpenTagName,
                @Attribs = @AttribStr, 
                @Text = NULL, --@Text,
                @OpenTagStartPos = @StartPos,
                @CloseTagEndPos = @EndPos, 
                @ParentDEID = @ParentDEID,
                @DEID = @LastDEID OUTPUT 
              END          
                            
            IF @ImmediateClose = 1 BEGIN         
              SET @ImmediateClose = 0
              --Note:  do not change @ParentDEID
            END
            ELSE BEGIN  
              IF @IsSingleton = 0 BEGIN
                --Note:  Comments, declarations and singleton tags should never be a parent,
                --and so they don't get pushed onto the stack.
                
                --Push tag            
                INSERT INTO @tvTagStack (Tag, DEID, ParentDEID)
                VALUES (@OpenTagName, @LastDEID, @ParentDEID)
                
                SET @ParentDEID = @LastDEID              
              END                                
            END               
                        

            SET @State = 'Text'
            
            SET @OpenTagName = ''              
            SET @AttribStr = ''
            SET @AttribChunk = ''
            SET @Text = ''
            SET @TextChunk = ''
            
          END


          --Processing for completed CloseTag
          IF @DoCloseTag = 1 BEGIN
            SET @DoCloseTag = 0
                              
            --Pop tag  
            IF @IsSingleton = 0 BEGIN
              --not a singleton tag
              
              SET @StackTag = ''     
              SET @PopDone = 0
                
              SELECT TOP (1) @TopStackID = TagStackID FROM @tvTagStack ORDER BY TagStackID DESC
                                
              WHILE (@TopStackID IS NOT NULL) AND 
                    (@StackTag <> @CloseTagName) AND
                    (@PopDone = 0) BEGIN
                    
                /*
                Note:  The idea is that we pushed nodes onto a stack.  We have reached the closing tag for a
                node, and so now we want to pop off all nodes that were pushed until we pop off the corresponding
                opening tag.
                
                There could be a problem is with non-XHTML:  In XMHTML, tags such as <td> and <li> must be 
                closed--as they should be--because they can contain child text nodes.  However, the HTML spec
                allows for <td> and <li> to be pseudo-singletons...meaning that they may not have a closing tag.
                
                Consequently, the current behavior is that since there is no closing tag (i.e. </td>) on the stack,
                we will keep poping until we come to the top of the stack.  Thus the next tag after the </td>--which
                will likely be a <td> in this case--will be inserted as a root-level node with no parent.
                
                This behavior is not bad:  it is fairly fault-tollerant.  The nodes will still be processed, and the
                sequence of the nodes will still be presevered.
                
                Nonetheless, a future enhancement might be to somehow limit the popping to stop at the "inferred" 
                parent.  For example, we know that the parent of a <td> should be a <tr>.  So perhaps stopping popping
                when we reach the <tr> is possible through some yet-to-be-defined means.
                */
                     
                SET @TopStackID = NULL
                SET @StackTag = NULL
                
                SELECT TOP (1)
                  @TopStackID = ts.TagStackID,
                  @StackTag = ts.Tag,
                  @LastDEID = ts.DEID,
                  @ParentDEID = ts.ParentDEID
                FROM
                  @tvTagStack ts
                ORDER BY
                  ts.TagStackID DESC      
                                                            
                DELETE FROM @tvTagStack WHERE TagStackID = @TopStackID

              END
            END
            
            UPDATE #tblDOM 
            SET CloseTagEndPos = @EndPos
            WHERE
              DEID = @LastDEID
            
            SET @CloseTagName = ''          
                        
            SET @State = 'Text'
            SET @Text = ''
                      
          END
        
        END 

        SET @i = @i + 1  
      END  
    END
   
    DELETE FROM @tvTargetList WHERE DEID = @DEID

    SET @DEID = NULL
        
    IF EXISTS(SELECT DEID FROM @tvTargetList) BEGIN
      SELECT TOP (1) @DEID = DEID FROM @tvTargetList
    END
  END
  
END

GO

/*
**************************************************************************************
PROCEDURE sdom.spgetHTTP
Procedure sdom.spgetHTTP is to retrieve data from a remote HTTP server
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.sputilGetHTTP
@URL varchar(MAX),
  --URL to retrieve data from
@HTTPMethod varchar(40) = 'GET',
  --can be either GET or POST
@ContentType varchar(80)= 'text/http',
  --set to 'application/x-www-form-urlencoded' for POST, etc.  
@DataToSend nvarchar(4000) = NULL, 
  --data to post, if @HTTPMethod = 'POST'   
@HTTPStatus int = NULL OUTPUT,
  --HTTP Status Code (200=OK, 404=Not Found, etc.)
@ResponseText nvarchar(MAX) = NULL OUTPUT,
  --Full text returned by remote HTTP server (if @SuppressResponseText = 0)

@ErrorMsg varchar(MAX) = NULL OUTPUT,
  --NULL unless an error message was encountered
@LastResultCode int = NULL OUTPUT,
  --0 unless an error code was returned by MSXML2.ServerXMLHttp

@User varchar(512) = NULL,
  --If provided, use this value for the HTTP authentication user name
@Password varchar(512) = NULL,
  --If provided, use this value for the HTTP authentication password
   
@SuppressResponseText bit = 0,
  --If 0, actual content is not returned from remote server (just status code)
@SuppressResultset bit = 1,
  --If 0, result set is is not returned (just parameters)
@SilenceErrors bit = 0
  --If 1, errors are not raised with RAISEERROR(), but caller can checn @ErrorMsg.
  --@ErrorMsg will be null if no error was raised.  
  
--Written by David Rueter (drueter@assyst.com)
AS 
BEGIN
  SET NOCOUNT ON
  
  --Retrieves data via HTTP 

  --http://msdn.microsoft.com/en-us/library/aa238861(v=sql.80).aspx
  
  SET NOCOUNT ON
  
  DECLARE @Debug bit
  SET @Debug = 0
  
  DECLARE @CRLF varchar(5)
  SET @CRLF = CHAR(13) + CHAR(10)

  DECLARE @Obj int 
  
  DECLARE @PerformedInit bit
  SET @PerformedInit = 0  

  DECLARE @ErrSource varchar(512)
  DECLARE @ErrMsg varchar(512)
  
  DECLARE @tvResponse TABLE (Response nvarchar(MAX))

  IF @Debug = 1 PRINT 'About to call sp_OACreate for MSXML2.ServerXMLHttp'
  
  BEGIN TRY
    EXEC @LastResultCode = sp_OACreate 'MSXML2.ServerXMLHttp', @Obj OUT 
    IF @LastResultCode <> 0 BEGIN
      EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
    END
    ELSE BEGIN
      SET @PerformedInit = 1 
    END
  END TRY
  BEGIN CATCH
    SET @ErrorMsg = ERROR_MESSAGE()
  END CATCH

  BEGIN TRY
     
    IF @LastResultCode = 0 BEGIN
      IF @HTTPMethod = 'GET' BEGIN

       IF @Debug = 1 PRINT 'About to call sp_OAMethod for open (GET)'      
        EXEC @LastResultCode = sp_OAMethod @Obj, 'open', NULL, 'GET', @URL, false, @User, @Password
        IF @LastResultCode <> 0 BEGIN
          EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
        END
                
      END
      ELSE BEGIN
       IF @Debug = 1 PRINT 'About to call sp_OAMethod for open (POST)'         
        EXEC @LastResultCode = sp_OAMethod @Obj, 'open', NULL, 'POST', @URL, false, @User, @Password
        IF @LastResultCode <> 0 BEGIN
          EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
        END
                     
        IF @Debug = 1 PRINT 'About to call sp_OAMethod for setRequestHeader'                        
        IF @LastResultCode = 0 EXEC @LastResultCode = sp_OAMethod @Obj, 'setRequestHeader', NULL, 'Content-Type', @ContentType
        IF @LastResultCode <> 0 BEGIN
          EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
        END
                       
      END
    END

    IF @Debug = 1 PRINT 'About to call sp_OAMethod for send'           
    IF @LastResultCode = 0 EXEC @LastResultCode = sp_OAMethod @Obj, 'send', NULL, @DataToSend
    IF @LastResultCode <> 0 BEGIN
      EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
    END    
    
    IF @LastResultCode = 0 EXEC @LastResultCode = sp_OAGetProperty @Obj, 'status', @HTTPStatus OUT 
    IF @LastResultCode <> 0 BEGIN
      EXEC sp_OAGetErrorInfo @obj, @ErrSource OUTPUT, @ErrMsg OUTPUT 
    END        
    
    IF (@LastResultCode = 0) AND (ISNULL(@SuppressResponseText, 0) = 0) BEGIN
      INSERT INTO @tvResponse (Response)
      EXEC @LastResultCode = sp_OAGetProperty @Obj, 'responseText' --, @Response OUT 
        --Note:  sp_OAGetProperty (or any extended stored procedure parameter) does not support
        --varchar(MAX), however returning as a resultset will return long results.      
    END
  END TRY
  BEGIN CATCH
   SET @ErrorMsg = ERROR_MESSAGE()
  END CATCH

  DECLARE @DestroyResultCode int
  EXEC @DestroyResultCode = sp_OADestroy @Obj

  SELECT @ResponseText = Response FROM @tvResponse
  
  SET @ErrorMsg = 
    NULLIF(RTRIM(
      ISNULL(@ErrorMsg, '') + 
      ISNULL(' (' + @ErrMsg + ')', '') + 
      ISNULL(' [' + @ErrSource + ']', '')
    ), '')


  IF @ErrorMsg IS NOT NULL BEGIN
    SET @ErrorMsg = 'Error in sputilGetHTTP: ' + @ErrorMsg
      
    IF @PerformedInit = 0 BEGIN
      SET @ErrorMsg = @ErrorMsg + @CRLF +
        'Remember that this stored procedure uses OLE.  To work properly you may need to configure ' +
        'your database to allow OLE, as follows: ' + @CRLF +
        '  EXEC sp_configure ''show advanced options'', 1;' + @CRLF +
        '  RECONFIGURE;' + @CRLF +      
        '  EXEC sp_configure ''Ole Automation Procedures'', 1;' + @CRLF +
        '  RECONFIGURE;' + @CRLF +
        'Also, your SQL user must have execute rights to the following stored procedures in master:' + @CRLF +
        '  sp_OACreate' + @CRLF +
        '  sp_OAGetProperty' + @CRLF +
        '  sp_OASetProperty' + @CRLF +
        '  sp_OAMethod' + @CRLF +
        '  sp_OAGetErrorInfo' + @CRLF +
        '  sp_OADestroy' + @CRLF +  
        'You can grant rights for each of these as follows:' + @CRLF +
        '  USE master' + @CRLF +
        '  GRANT EXEC ON sp_OACreate TO myuser' + @CRLF +
        '  GRANT EXEC etc. ...'  
        
      IF ISNULL(@SilenceErrors, 0) = 0 BEGIN
        RAISERROR(@ErrorMsg, 16, 1)
      END
    END      
  END
    
  IF ISNULL(@SuppressResultset, 0) = 0 BEGIN
    SELECT 
      @URL AS URL,
      @ResponseText AS ResponseText,
      @HTTPStatus AS HTTPStatus,
      @LastResultCode AS LastResultCode,
      @ErrorMsg AS ErrorMsg
  END

END

GO
-- Spi: 75 statements.

/*
**************************************************************************************
PROCEDURE sdom.sputilConvertJSONToXML
Procedure sdom.sputilConvertJSONToXML is to convert JSON data to XML
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.sputilConvertJSONToXML
@JSON nvarchar(MAX),
@XML xml OUTPUT
AS
BEGIN
  SET NOCOUNT ON
  
  DECLARE @tvStack TABLE (
    StackID int IDENTITY PRIMARY KEY, 
    Tag varchar(8000),
    IsArrayElem bit  
  )

  DECLARE @I int
  DECLARE @C char
  DECLARE @LastChar char

  DECLARE @Buf varchar(8000)
  DECLARE @XMLStr varchar(MAX)
  DECLARE @Tag varchar(8000)

  DECLARE @StackID int

  DECLARE @InQuote bit
  DECLARE @EndedQuote bit
  DECLARE @IsArrayElem bit

  SET @I = 1
  SET @InQuote = 0

  SET @XMLStr = ''
  SET @Buf = ''

  WHILE @I < LEN(@JSON + 'x') - 1 BEGIN
    IF @C NOT IN (CHAR(9), CHAR(10), CHAR(13), ' ') SET @LastChar = @C
    
    SET @C = SUBSTRING(@JSON, @I, 1)
    
    IF @C = '"' BEGIN
      --Found Quote
      IF @EndedQuote = 1 BEGIN
        --Just exited a quote:  special case for embedded ""   
        SET @Buf = @Buf + @C
        SET @InQuote = 1
        SET @EndedQuote = 0
      END
      ELSE IF @InQuote = 1 BEGIN
        --We were already in a quote, so we must be exiting
        SET @InQuote = 0
        SET @EndedQuote = 1
      END
      ELSE BEGIN
        SET @InQuote = 1
      END
    END
    ELSE BEGIN
      --not a quote character
      
      SET @EndedQuote = 0
      IF (@InQuote = 1) BEGIN
        --just append character
        IF @C NOT IN (CHAR(9), CHAR(10), CHAR(13)) BEGIN
          SET @Buf = @Buf + 
            CASE @C 
              WHEN '<' THEN '&lt;'
              WHEN '>' THEN '&gt;'
              WHEN '&' THEN '&amp;'
              ELSE @C
            END
        END
      END
      ELSE BEGIN
        --inspect character to determine state
        
        IF @C = ':' BEGIN
          --@Buf contains VarName
          SET @XMLStr = @XMLStr + '<' + @Buf + '>'
          
          INSERT INTO @tvStack (Tag) VALUES (@Buf)     
          
          SET @Buf = ''
        END
        ELSE IF @C = ',' BEGIN      
          --@Buf contains VarValue         
          IF @Buf <> '' BEGIN
            SET @XMLStr = @XMLStr + @Buf
            SET @Buf = ''          
          
            --pop tag from stack and write closing tag to XML
            SET @Tag = ''
            SELECT TOP (1) @Tag = Tag, @StackID = StackID FROM @tvStack ORDER BY StackID DESC      
            DELETE FROM @tvStack WHERE StackID = @StackID
          
            IF @Tag <> '' BEGIN
              SET @XMLStr = @XMLStr + '</' + @Tag + '>'   
            END
          END 

          --We are on a comma.  If the top element is an array element, peek and write
          --a close tag and a re-open tag to XML
          SET @IsArrayElem = 0
          SELECT TOP (1) @IsArrayElem = IsArrayElem, @Tag = Tag FROM @tvStack ORDER BY StackID DESC   
          IF @LastChar = '}' AND @IsArrayElem = 1 BEGIN
            SET @XMLStr = @XMLStr + '</' + @Tag +'>' + '<' + @Tag + '>'                  
          END             
                   
        END   
        ELSE IF @C = '[' BEGIN
          --Start of array.  
          
          --peek at stack and add first array element tag
          SET @Tag = ''
          SELECT TOP (1) @Tag = Tag, @StackID = StackID FROM @tvStack ORDER BY StackID DESC     
          
          IF @Tag <> '' BEGIN
            SET @Tag = @Tag + '_'
      
            --push array element tag to stack and write closing tag to XML    
            INSERT INTO @tvStack (Tag, IsArrayElem) VALUES (@Tag, 1)                     
            SET @XMLStr = @XMLStr + '<' + @Tag + '>'     
          END
        END
        ELSE IF @C = '}' BEGIN
          --at end of object
          
          --pop tag from stack and write closing tag to XML
          SELECT TOP (1) @Tag = Tag, @StackID = StackID FROM @tvStack ORDER BY StackID DESC     
          DELETE FROM @tvStack WHERE StackID = @StackID
          
          IF @Tag <> '' BEGIN
            SET @XMLStr = @XMLStr + @Buf + '</' + @Tag + '>'                  
          END
          SET @Buf = ''                      
        END
        ELSE IF @C = ']' BEGIN                 
          SELECT TOP (1) @Tag = Tag, @StackID = StackID FROM @tvStack ORDER BY StackID DESC     
          DELETE FROM @tvStack WHERE StackID = @StackID
          
          IF @Tag <> '' BEGIN
            SET @XMLStr = @XMLStr + @Buf + '</' + @Tag + '>'                  
          END
          SET @Buf = ''
        END             
        ELSE BEGIN
          IF @C NOT IN (CHAR(9), CHAR(10), CHAR(13), '{') BEGIN      
            SET @Buf = @Buf +
              CASE @C 
                WHEN '<' THEN '&lt;'
                WHEN '>' THEN '&gt;'
                WHEN '&' THEN '&amp;'
                WHEN ' ' THEN ''
                ELSE @C
              END            
          END
        END
        
      END
    END
    
    SET @I = @I + 1
  END

  --pop any remaining tags from stack
  WHILE EXISTS(SELECT StackID FROM @tvStack) BEGIN
    SET @Tag = ''
    SELECT TOP (1) @Tag = Tag, @StackID = StackID FROM @tvStack ORDER BY StackID DESC      
    DELETE FROM @tvStack WHERE StackID = @StackID
    IF @Tag <> '' BEGIN
      SET @XMLStr = @XMLStr + '</' + @Tag + '>'
    END
  END

  SET @XML = NULLIF(RTRIM(@XMLStr), '')
END
GO

/*
**************************************************************************************
PROCEDURE sdom.spgetText
Procedure  sdom.spgetText is for convenience, to retrieve the text node specified by
HUID, ID, Name, or Class.  If @Attrib is specified, this refers to the attribute
name of the element specified--and the attribute value is returned.
sdom.spgetDOM MUST have been called first prior to calling sdom.spgetText.
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spgetText
@HUID varchar(900) = NULL,
@ID varchar(512) = NULL,
@Name varchar(512) = NULL,
@Class varchar(512) = NULL,
@Attrib varchar(512) = NULL,
@TextData varchar(MAX) = NULL OUTPUT,
@SuppressRecordset bit = 1
AS
BEGIN
  SET NOCOUNT ON
  
  SET @TextData = NULL    
  
  IF @Attrib IS NULL BEGIN
    SELECT
      @TextData = dh.TextData
    FROM
      #tblDOMHierarchy dh
    WHERE
      (
       ((@HUID IS NOT NULL) AND (dh.HUID = @HUID)) OR
       ((@ID IS NOT NULL) AND (dh.ID = @ID)) OR
       ((@Name IS NOT NULL) AND (dh.Name = @Name)) OR
       ((@Class IS NOT NULL) AND (dh.Class = @Class))
      )
  END
  ELSE BEGIN
    SELECT
      @TextData = at.Value
    FROM
      #tblDOMHierarchy dh
      JOIN #tblDOMAttribs at ON
        dh.DEID = at.DEID AND
        at.Name = @Attrib
    WHERE
      (
       ((@HUID IS NOT NULL) AND (dh.HUID = @HUID)) OR
       ((@ID IS NOT NULL) AND (dh.ID = @ID)) OR
       ((@Name IS NOT NULL) AND (dh.Name = @Name)) OR
       ((@Class IS NOT NULL) AND (dh.Class = @Class))
      )
      
  END

  SET @TextData = 
   LTRIM(
   RTRIM(
   REPLACE(
   REPLACE(
   REPLACE(
   REPLACE(
   REPLACE(@TextData,   
     '&nbsp;', ' '),
     '&quot;', '"'),     
     CHAR(9), ' '),
     CHAR(10), ' '),
     CHAR(13), ' ')
   ))
   
  IF @SuppressRecordset = 0 BEGIN
    SELECT @TextData AS TextData
  END   
     
END
GO

-- Spi: 81 statements.

/*
**************************************************************************************
PROCEDURE sdom.spgetInitSession
Procedure sdom.spgetInitSesion returns the HTML the caller needs to run to
create the temporary tables for SQLDOM to use.
**************************************************************************************
*/
GO
CREATE PROCEDURE sdom.spgetInitSession
@SQLToExecute varchar(MAX) = NULL OUTPUT
--$!ParseMarker
--Note:  comments and code between marker and AS are subject to automatic removal by OpsStream
--©Copyright 2006-2010 by David Rueter, Automated Operations, Inc.
--May be held, used or transmitted only pursuant to an in-force licensing agreement with Automated Operations, Inc.
--Contact info@opsstream.com / 800-964-3646 / 949-264-1555
WITH EXECUTE AS OWNER
AS 
BEGIN 
SET NOCOUNT ON
  
SET @SQLToExecute =  
------------
'IF OBJECT_ID(''tempdb..#tblDOMDocs'') IS NOT NULL BEGIN
  DROP TABLE #tblDOMDocs
END

IF OBJECT_ID(''tempdb..#tblDOM'') IS NOT NULL BEGIN
  DROP TABLE #tblDOM
END

IF OBJECT_ID(''tempdb..#tblDOMAttribs'') IS NOT NULL BEGIN
  DROP TABLE #tblDOMAttribs
END

IF OBJECT_ID(''tempdb..#tblDOMStyles'') IS NOT NULL BEGIN
  DROP TABLE #tblDOMStyles
END

IF OBJECT_ID(''tempdb..#tblDOMHierarchy'') IS NOT NULL BEGIN
  DROP TABLE #tblDOMHierarchy
END


/*
**************************************************************************************
TABLE #tblDOMDocs
Table #tblDOMDocs is for list of DOM documents (groups of tblDOM rows).
**************************************************************************************
*/

CREATE TABLE #tblDOMDocs(
DocID int identity PRIMARY KEY,
DateCreated datetime,
DocName varchar(128)
)

GO

/*
**************************************************************************************
TABLE  #tblDOM
Table #tblDOM  is for internal representation of the DOM data
**************************************************************************************
*/
CREATE TABLE #tblDOM (
  DEID int identity PRIMARY KEY,
  DocID int,  
  Tag varchar(MAX),
  ID varchar(512),
  Name varchar(512),  
  Class varchar(512),
  TextData varchar(MAX),
  OpenTagStartPos int,
  CloseTagEndPos int,
  ParentDEID int
)

CREATE INDEX ixDOMTable_ParentDEID ON #tblDOM (ParentDEID, DEID)
CREATE INDEX ixDOMTable_DocID_ParentDEID ON #tblDOM (DocID, ParentDEID, DEID)
CREATE INDEX ixDOMTable_DEID ON #tblDOM (DEID, DocID)

--NOTE: SQL 2008 introduced filtered indexes, which makes it easy to enforce
--unqique-but-nullable. If on SQL 2008 or greater AND you wish to enforce uniqueness
--of ID and Name attributes, uncomment the following two lines
--  CREATE UNIQUE INDEX tmpixDOMTable_ID ON #tblDOM (ID) INCLUDE (DEID) WHERE ID IS NOT NULL
--  CREATE UNIQUE INDEX tmpixDOMTable_Name ON #tblDOM (Name) INCLUDE (DEID) WHERE Name IS NOT NULL





/*
Note:
TextData will contain the data for the first text node (if any) under the tag.
Subsequent text nodes (if any) will be in their own #tblDOM row, with a null TAG
and referencing the original DEID in the ParentDEID column.
*/


GO

/*
**************************************************************************************
TABLE #DOMAttribs
Table #tblDOMAttribs is for internal representation of the DOM data--specifically,
for attributes of DOM elements
**************************************************************************************
*/
CREATE TABLE #tblDOMAttribs(
DOMAttribID int identity PRIMARY KEY,
DEID int,
Name varchar(512),
Value varchar(MAX)
)

CREATE UNIQUE INDEX uqDOMAttribs_DEID ON #tblDOMAttribs (DEID, Name)
CREATE INDEX ixDOMAttribs_DEID ON #tblDOMAttribs (DEID) INCLUDE (Name, Value)

GO

/*
**************************************************************************************
TABLE #tblDOMStyles
Table #tblDOMAttribs is for internal representation of the DOM data--specifically,
for attributes of DOM elements
**************************************************************************************
*/
CREATE TABLE #tblDOMStyles(
DOMStyleID int identity PRIMARY KEY,
DEID int,
Name varchar(512),
Value varchar(MAX)
)

CREATE UNIQUE INDEX ixDOMStyles_ID ON #tblDOMStyles (DEID, Name)
CREATE INDEX ixDOMStyles_DEID ON #tblDOMStyles (DEID) INCLUDE (Name, Value)

GO


/*
**************************************************************************************
TABLE #tblDOMHierarchy
Table #tblDOMHierarchy is a table that automatically caches the output of #spgetDOM
Most of the data is reduncant to what is in #tblDOM, but the 4 fields HUID,
SortHUID, DOMLevel, and Sequence are sufficiently useful to warrant this
duplication.  This table should be regarded as read-only and transitory.  Do not
update.
**************************************************************************************
*/
CREATE TABLE #tblDOMHierarchy(
  DEID int PRIMARY KEY,
  DocID int,
  Tag varchar(MAX),
  ID varchar(512),
  Name varchar(512),
  Class varchar(512),
  TextData varchar(MAX),
  OpenTagStartPos int,
  CloseTagEndPos int,
  ParentDEID int,
  --fields not present in #tblDOM:
  HUID varchar(900),
  SortHUID varchar(900),
  DOMLevel int,
  Sequence int,
  HasChild bit
)

CREATE INDEX ixDOMHierarchy_ParentDEID ON #tblDOMHierarchy (ParentDEID, DEID)
CREATE INDEX ixDOMHierarcy_HUID ON #tblDOMHierarchy (HUID) INCLUDE (DEID)
CREATE INDEX ixDOMHierarchy_SortHUID ON #tblDOMHierarchy (SortHUID) INCLUDE (DEID)
CREATE INDEX ixDOMHierarchy_Sequence ON #tblDOMHierarchy (Sequence, DEID)
GO
'

 CREATE TABLE #InitSQL (
  InitID int IDENTITY PRIMARY KEY,
  SQLToExecute varchar(MAX)
  )
  
  INSERT INTO #InitSQL (SQLToExecute)
  VALUES(@SQLToExecute)

  SELECT * FROM #InitSQL
  ORDER BY InitID  
  
  PRINT @SQLToExecute

END
GO
------------------------------------------------
PRINT '
SQLDOM version .927 has been successfully loaded and is ready for use.

In this version, stored procedures are standard persistent stored procedures that access temporary tables.
Since the scope of the temporary tables needs to persist across all calls to the SQLDOM procedures on a database
connection, the caller must explicitly create the temporary tables on the connection before calling SQLDOM.

To obtain the SQL statement to create the temporary tables that needs to be called by the caller, do this:

    EXEC sdom.spgetInitSession
    
This returns the needed SQL in three ways:  in the @SQLToExecute parameter, in the SQLToExecute column in
the resultset returned, and output via a PRINT statement.  Execute this code on the connection before attempting to
use SQLDOM.  REMEMBER: you can NOT do this in a call like: EXEC(@SQLToExecute) because the scope of the temporary
tables would be limited to the EXEC() statement--the temp tables would be immedately dropped after the EXEC()
statement.  You will likely need to copy-and-paste the SQLToExec code and manually execute that before you make
calls to SQLDOM.

(Temp tables HAVE been created on this connection:  you can immediately try out SQLDOM here with no further
initialization.  But you will need to create the temp tables yourself on any new SQL connection.)

Things to try:

--Example 1:  Simple parse of string
EXEC sdom.spactDOMLoad @HTML = ''<html><body>Hello World.<br /><div><p>SQLDOM <b>ROCKS!</b></p></div></body></html>''
EXEC sdom.spgetDOM 

--Example 2:  Render HTML from DOM (that we parsed in Example 1 above)
EXEC sdom.spgetDOMHTML @PrettyWhitespace=1, @PrintHTML = 1

--Example 3:  Parse and re-render from a URL
DECLARE @HTML varchar(MAX)

EXEC sdom.sputilGetHTTP
  @URL = ''http://www.google.com'',
  @ResponseText = @HTML OUTPUT,
  @SuppressResultset = 1  

EXEC sdom.spactDOMLoad @HTML=@HTML
EXEC sdom.spgetDOM 
EXEC sdom.spgetDOMHTML @PrettyWhitespace=1, @PrintHTML = 1

--Example 4:  Parse from a string, modify the DOM, render resulting HTML

EXEC sdom.spactDOMLoad @HTML = ''<html><body>Hello World.<br /><div id="myContent">Future content goes here</div></body></html>''

EXEC sdom.spactDOMLoad @HTML = ''<div>Here is some neat stuff about <b>SQLDOM</b></div>'', @Selector = ''.myContent''

EXEC sdom.spgetDOM 
EXEC sdom.spgetDOMHTML @PrettyWhitespace=1, @PrintHTML = 1
'

print 'Spi: 86 statements including this one.'