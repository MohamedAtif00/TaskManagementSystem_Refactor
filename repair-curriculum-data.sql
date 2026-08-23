-- Repairs curriculum data migrated from the legacy 3-level Folders tree
-- (Season > Term > Subject family) into the 4-level hierarchy
-- (Year > Project > Term > Subject Group).
--
-- Before: "Term 1"/"Term 2" sit at the Project level and the subject
-- families (Arabic, Math, ...) sit at the Term level.
-- After:  Year > Selah Eltelmeez > Term 1 / Term 2 > subject groups,
-- with subjects re-assigned to groups by their naming convention.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @yearId INT = (SELECT TOP 1 Id FROM AcademicYears ORDER BY Id);
DECLARE @projId INT = (SELECT TOP 1 Id FROM CurriculumProjects WHERE YearId = @yearId AND Name = N'Selah Eltelmeez' ORDER BY Id);
DECLARE @fakeT1 INT = (SELECT TOP 1 Id FROM CurriculumProjects WHERE YearId = @yearId AND Name = N'Term 1');
DECLARE @fakeT2 INT = (SELECT TOP 1 Id FROM CurriculumProjects WHERE YearId = @yearId AND Name = N'Term 2');

IF @projId IS NULL OR @fakeT1 IS NULL OR @fakeT2 IS NULL
BEGIN
    RAISERROR (N'Expected project rows not found; aborting.', 16, 1);
    ROLLBACK;
    RETURN;
END;

-- 1. Real terms under the real project.
IF NOT EXISTS (SELECT 1 FROM CurriculumTerms WHERE ProjectId = @projId AND Name = N'Term 1')
    INSERT INTO CurriculumTerms (ProjectId, Name) VALUES (@projId, N'Term 1');
IF NOT EXISTS (SELECT 1 FROM CurriculumTerms WHERE ProjectId = @projId AND Name = N'Term 2')
    INSERT INTO CurriculumTerms (ProjectId, Name) VALUES (@projId, N'Term 2');

DECLARE @term1 INT = (SELECT Id FROM CurriculumTerms WHERE ProjectId = @projId AND Name = N'Term 1');
DECLARE @term2 INT = (SELECT Id FROM CurriculumTerms WHERE ProjectId = @projId AND Name = N'Term 2');

-- 2. Subject groups under each real term, using the family names that
--    currently sit (incorrectly) at the term level under the fake projects.
DECLARE @names TABLE (Name NVARCHAR(200) PRIMARY KEY);
INSERT INTO @names
SELECT DISTINCT Name FROM CurriculumTerms WHERE ProjectId IN (@fakeT1, @fakeT2);

INSERT INTO SubjectGroups (TermId, Name)
SELECT t.TermId, n.Name
FROM @names n
CROSS JOIN (VALUES (@term1), (@term2)) t(TermId)
WHERE NOT EXISTS (
    SELECT 1 FROM SubjectGroups sg WHERE sg.TermId = t.TermId AND sg.Name = n.Name);

-- 3. Re-assign every subject to its group using the naming convention:
--    prefix -> family (ara/eng/mth/sci/soc/ict/mul/rel/tsk), otherwise Other;
--    suffix _2a/_2e or "term 2" in the name -> Term 2, otherwise Term 1.
;WITH SubjectTarget AS (
    SELECT
        s.Id,
        CASE
            WHEN LOWER(s.Name) LIKE 'ara%' THEN N'Arabic'
            WHEN LOWER(s.Name) LIKE 'eng%' THEN N'English'
            WHEN LOWER(s.Name) LIKE 'mth%' OR LOWER(s.Name) LIKE 'math%' THEN N'Math'
            WHEN LOWER(s.Name) LIKE 'sci%' THEN N'Science'
            WHEN LOWER(s.Name) LIKE 'soc%' THEN N'Social Studies'
            WHEN LOWER(s.Name) LIKE 'ict%' THEN N'ICT'
            WHEN LOWER(s.Name) LIKE 'mul%' THEN N'Multimedia'
            WHEN LOWER(s.Name) LIKE 'rel%' THEN N'Religion'
            WHEN LOWER(s.Name) LIKE 'tsk%' THEN N'Tokkatsu'
            ELSE N'Other'
        END AS GroupName,
        CASE
            WHEN LOWER(RTRIM(s.Name)) LIKE '%[_]2[ae]' THEN @term2
            WHEN LOWER(s.Name) LIKE '%term%2%' THEN @term2
            ELSE @term1
        END AS TermId
    FROM Subjects s
)
UPDATE s
SET s.SubjectGroupId = sg.Id
FROM Subjects s
INNER JOIN SubjectTarget st ON st.Id = s.Id
INNER JOIN SubjectGroups sg ON sg.TermId = st.TermId AND sg.Name = st.GroupName;

-- 4. Remove the fake structure now that nothing references it.
DELETE FROM SubjectGroups
WHERE TermId IN (SELECT Id FROM CurriculumTerms WHERE ProjectId IN (@fakeT1, @fakeT2));

DELETE FROM CurriculumTerms WHERE ProjectId IN (@fakeT1, @fakeT2);

DELETE FROM CurriculumProjects WHERE Id IN (@fakeT1, @fakeT2);

COMMIT;

-- Verification output.
SELECT p.Name AS Project, t.Name AS Term, sg.Name AS SubjectGroup, COUNT(s.Id) AS Subjects
FROM CurriculumProjects p
INNER JOIN CurriculumTerms t ON t.ProjectId = p.Id
INNER JOIN SubjectGroups sg ON sg.TermId = t.Id
LEFT JOIN Subjects s ON s.SubjectGroupId = sg.Id
GROUP BY p.Name, t.Name, sg.Name
ORDER BY p.Name, t.Name, sg.Name;
