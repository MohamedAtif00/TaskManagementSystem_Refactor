-- Listed LOs where Archived = 0, and the name still has work open:
-- at least one non-archived LO with that name has a task with Status <> 3.
--
-- RepeatCount counts every non-archived LO with the same name, including
-- the sibling whose tasks are all Done. Example:
--   LO A and LO B, both Archived = 0, same name
--   A has at least one incomplete task, B is fully Done
--   -> both rows are returned, RepeatCount = 2, Repeated = 1
--
-- Matching is case-insensitive and ignores spaces
-- (e.g. 'mth_1r_1a _02_02_08' vs 'mth_1r_1a_02_02_08').
--
-- TaskStatusEnum: 0=Backlog, 1=ToDo, 2=Doing, 3=Done, 4=Rollback
-- Incomplete = Status <> 3. Archived tasks are ignored.

SET NOCOUNT ON;

DECLARE @LoList NVARCHAR(MAX) = N'
2026_mth_1r_1a_01_04_01
2026_mth_1r_1a_01_04_02
2025_mth_1r_1a_01_03_02
2026_mth_1r_1a_01_05_01
mth_1r_1a _02_02_08
QR_mth_1r_1a_02_02_06
mth_1r_1a _02_02_09
2026_mth_1r_1a_01_06_01
2026_mth_1r_1a_01_06_02
2026_mth_1r_1a_01_07_01
2026_mth_1r_1a_01_07_02
2025_mth_1r_1a_01_05_02
2026_mth_1r_1a_01_08_01
2026_mth_1r_1a_01_08_02
2025_mth_1r_1a_01_05_04
2026_mth_1r_1a_01_09_01
2026_mth_1r_1a_01_09_02
2025_mth_1r_1a_01_05_06
2026_mth_1r_1a_01_10_01
2026_mth_1r_1a_01_10_02
2025_mth_1r_1a_01_06_02
2026_mth_1r_1a_01_11_01
2025_mth_1r_1a_01_06_04
QR_mth_1r_2a_04_04_03
mth_1r_1a_03_01_07
mth_1r_1a_03_01_08
mth_1r_1a_03_01_09
mth_1r_1a_03_01_10
mth_1r_1a_03_01_013
mth_1r_1a_03_01_014
2026_mth_1r_1a_01_13_01
mth_1r_1a_01_04_02
2025_mth_1r_1a_01_08_02
2026_mth_1r_1a_01_14_02
2026_mth_2r_1a_01_01_01
2026_mth_2r_1a_01_01_02
2026_mth_2r_1a_01_01_03
2026_mth_2r_1a_01_01_04
2026_mth_2r_1a_01_01_05
2026_mth_2r_1a_01_02_01
2026_mth_2r_1a_01_02_02
2026_mth_2r_1a_01_02_04
2026_mth_2r_1a_01_03_01
2026_mth_2r_1a_01_03_02
2026_mth_2r_1a_01_03_03
2026_mth_2r_1a_01_03_05
2026_mth_2r_1a_01_03_07
2026_mth_2r_1a_01_03_09
2026_mth_2r_1a_01_03_10
2026_mth_2r_1a_01_04_01
2026_mth_2r_1a_01_04_02
2026_mth_3r_1a_01_01_01
2026_mth_3r_1a_01_01_02
2026_mth_3r_1a_01_01_04
2026_mth_3r_1a_01_01_05
2026_mth_3r_1a_01_02_01
2026_mth_3r_1a_01_02_02
2026_mth_3r_1a_01_02_03
2026_mth_3r_1a_01_03_01
2026_mth_3r_1a_01_03_02
2026_mth_3r_1a_01_03_04
2026_mth_3r_1a_01_03_06
2026_mth_3r_1a_01_04_01
2026_mth_3r_1a_01_04_02
2026_mth_3r_1a_01_04_03
2026_mth_3r_1a_01_05_01
2026_mth_3r_1a_01_05_02
2026_mth_3r_1a_01_05_03
2026_mth_3r_1a_01_06_01
2026_mth_3r_1a_01_06_02
2025_soc_4r_1a_01_01_02
2026_soc_4r_1a_01_01_05
2025_soc_4r_1a_01_01_03
2026_soc_4r_1a_01_01_06
2025_soc_4r_1a_01_01_04
2025_soc_4r_1a_01_02_01
2026_soc_4r_1a_01_02_01
2026_soc_4r_1a_01_02_03
2026_soc_4r_1a_01_02_02
2025_soc_4r_1a_01_02_03
2025_soc_4r_1a_01_03_02
2026_soc_4r_1a_01_03_01
Soc_4R_1A_01_04_03_p2
Soc_4R_1A_01_01_03
2025_soc_4r_1a_01_03_04
2025_soc_4r_1a_01_04_01
2026_soc_4r_1a_01_04_02
2026_soc_4r_1a_01_04_03
Soc_4R_1A_01_02_05
2026_soc_4r_1a_01_04_04
2025_soc_4r_1a_01_04_03
2026_soc_4r_1a_01_05_01
2026_soc_4r_1a_01_05_02
2025_soc_4r_1a_01_05_01
2025_soc_4r_1a_01_05_02
2025_soc_5r_1a_01_01_01
2026_soc_5r_1a_01_01_01
2025_soc_5r_1a_01_01_02
2026_soc_5r_1a_01_01_02
2025_soc_5r_1a_01_01_03
2026_soc_5r_1a_01_01_04
2025_soc_5r_1a_01_01_06
2025_soc_5r_1a_01_01_05
2025_soc_5r_1a_01_01_07
2025_soc_5r_1a_01_02_01
2026_soc_5r_1a_01_02_03
2025_soc_5r_1a_01_02_02
2026_soc_5r_1a_01_02_04
2025_soc_5r_1a_01_02_03
2026_soc_5r_1a_01_02_06
2026_soc_5r_1a_01_02_08
2025_soc_5r_1a_01_02_07
2025_soc_5r_1a_01_02_09
2025_soc_5r_1a_01_02_06
2025_soc_5r_1a_01_02_08
Soc_4R_1A_02_02_01
2026_soc_5r_1a_01_03_01
2025_soc_5r_1a_01_03_01
2026_soc_5r_1a_01_03_04
2026_soc_5r_1a_01_03_06
2025_soc_5r_1a_01_03_06
2025_soc_5r_1a_01_03_07
2026_soc_5r_1a_01_04_01
2026_soc_5r_1a_01_04_02
2026_soc_5r_1a_01_04_03
2026_soc_5r_1a_01_04_04
2026_soc_5r_1a_01_04_05
2025_soc_5r_1a_01_04_08
2025_soc_5r_1a_01_04_07
2025_soc_5r_1a_01_04_09
2026_soc_5r_1a_01_05_01
2026_soc_5r_1a_01_05_02
2025_soc_5r_1a_01_05_01
2026_soc_6r_1a_01_01_01
2026_soc_6r_1a_01_01_02
2026_soc_6r_1a_01_01_03
2026_soc_6r_1a_01_01_08
2025_soc_6r_1a_01_01_05
2025_soc_6r_1a_01_01_07
2025_soc_6r_1a_01_01_06
soc_6r_1a_01_01_07
2025_soc_6r_1a_01_02_01
2026_soc_6r_1a_01_02_01
2026_soc_6r_1a_01_02_02
2026_soc_6r_1a_01_02_04
2026_soc_6r_1a_01_02_08
2025_soc_6r_1a_01_02_04
2025_soc_6r_1a_01_02_05
soc_6r_1a_02_01_08
2026_soc_6r_1a_01_03_02
2026_soc_6r_1a_01_03_03
2026_soc_6r_1a_01_03_07
2025_soc_6r_1a_01_03_07
2025_soc_6r_1a_01_03_06
soc_6r_1a_02_02_06
soc_6r_1a_02_03_08
2026_soc_6r_1a_01_04_01
2026_soc_6r_1a_01_04_02
2025_soc_6r_1a_01_04_04
2025_soc_6r_1a_01_04_05
soc_6r_1a_02_04_05
2026_soc_6r_1a_01_05_01
2026_soc_6r_1a_01_05_02
2025_soc_6r_1a_01_05_01
2026_sci_4r_1a_01_01_01
2026_sci_4r_1a_01_01_02
2026_sci_4r_1a_01_01_03
2026_sci_4r_1a_01_01_04
2026_sci_4r_1a_01_01_05
2026_sci_4r_1a_01_01_06
2026_sci_4r_1a_01_02_01
2026_sci_4r_1a_01_02_04
2026_sci_4r_1a_01_02_05
2026_sci_4r_1a_01_02_02
2026_sci_4r_1a_01_02_03
2026_sci_4r_1a_01_02_06
2026_sci_4r_1a_01_03_01
2026_sci_4r_1a_01_03_02
2026_sci_4r_1a_01_03_03
2026_sci_4r_1a_01_03_06
2026_sci_4r_1a_01_04_01
2026_sci_4r_1a_01_04_02
2026_sci_4r_1a_01_04_03
2026_sci_4r_1a_01_04_06
2026_sci_4r_1a_01_05_01
2026_sci_4r_1a_01_05_02
2026_sci_4r_1a_01_05_03
2026_sci_4r_1a_01_05_06
2026_sci_4r_1a_01_06_01
2026_sci_4r_1a_01_06_02
2026_sci_5r_1a_01_01_01
2026_sci_5r_1a_01_01_02
2026_sci_5r_1a_01_01_03
2026_sci_5r_1a_01_01_04
2026_sci_5r_1a_01_01_05
2026_sci_5r_1a_01_01_06
2026_sci_5r_1a_01_02_01
2026_sci_5r_1a_01_02_02
2026_sci_5r_1a_01_02_03
2026_sci_5r_1a_01_02_04
2026_sci_5r_1a_01_02_05
2026_sci_5r_1a_01_02_06
2026_sci_5r_1a_01_03_01
2026_sci_5r_1a_01_03_02
2026_sci_5r_1a_01_03_03
2026_sci_5r_1a_01_03_06
2026_sci_5r_1a_01_04_01
2026_sci_5r_1a_01_04_04
2026_sci_5r_1a_01_04_05
2026_sci_5r_1a_01_04_06
2026_sci_5r_1a_01_05_01
2026_sci_5r_1a_01_05_02
2026_sci_6r_1a_01_01_01
2026_sci_6r_1a_01_01_02
2026_sci_6r_1a_01_01_03
2026_sci_6r_1a_01_01_04
2026_sci_6r_1a_01_01_05
2026_sci_6r_1a_01_01_06
2026_sci_6r_1a_01_02_01
2026_sci_6r_1a_01_02_02
2026_sci_6r_1a_01_02_03
2026_sci_6r_1a_01_02_04
2026_sci_6r_1a_01_02_05
2026_sci_6r_1a_01_02_06
2026_sci_6r_1a_01_03_01
2026_sci_6r_1a_01_03_02
2026_sci_6r_1a_01_03_03
2026_sci_6r_1a_01_03_06
2026_sci_6r_1a_01_04_01
2026_sci_6r_1a_01_04_02
2026_mth_1r_1e_01_01_01
2026_mth_1r_1e_01_01_02
2026_mth_1r_1e_01_02_01
2026_mth_1r_1e_01_02_02
mth_1r_1e_01_02_02
2026_mth_1r_1e_01_03_01
QR_mth_1r_1e_01_02_03
mth_1r_1e _01_04_03
mth_1r_1e _01_04_05
mth_1r_1e _01_04_06
2026_mth_1r_1e_01_04_01
2026_mth_1r_1e_01_04_02
2025_mth_1r_1e_01_03_02
2026_mth_1r_1e_01_05_01
mth_1r_1e _02_01_08
mth_1r_1e _02_02_08
QR_mth_1r_1e_02_02_06
mth_1r_1e _02_02_09
2026_mth_1r_1e_01_06_01
2026_mth_1r_1e_01_06_02
2026_mth_1r_1e_01_07_01
2026_mth_1r_1e_01_07_02
2025_mth_1r_1e_01_05_02
2026_mth_1r_1e_01_08_01
2026_mth_1r_1e_01_08_02
2025_mth_1r_1e_01_05_04
2026_mth_1r_1e_01_09_01
2026_mth_1r_1e_01_09_02
2025_mth_1r_1e_01_05_06
2026_mth_1r_1e_01_10_01
2026_mth_1r_1e_01_10_02
2025_mth_1r_1e_01_06_02
2026_mth_1r_1e_01_11_01
2025_mth_1r_1e_01_06_04
QR_mth_1r_2e_04_04_03
mth_1r_1e _03_01_07
mth_1r_1e _03_01_08
mth_1r_1e _03_01_09
mth_1r_1e _03_01_10
mth_1r_1e _03_01_013
mth_1r_1e _03_01_014
2026_mth_1r_1e_01_13_01
mth_1r_1e _01_04_02
2025_mth_1r_1e_01_08_02
2026_mth_1r_1e_01_14_01
2026_mth_1r_1e_01_14_02
2026_mth_2r_1e_01_01_02
2026_mth_2r_1e_01_01_03
2026_mth_2r_1e_01_01_04
2026_mth_2r_1e_01_01_05
2026_mth_2r_1e_01_02_01
2026_mth_2r_1e_01_02_02
2026_mth_2r_1e_01_02_04
2026_mth_2r_1e_01_03_01
2026_mth_2r_1e_01_03_02
2026_mth_2r_1e_01_03_03
2026_mth_2r_1e_01_03_05
2026_mth_2r_1e_01_03_07
2026_mth_2r_1e_01_03_09
2026_mth_2r_1e_01_03_10
2026_mth_2r_1e_01_04_01
2026_mth_2r_1e_01_04_02
2026_mth_3r_1e_01_01_01
2026_mth_3r_1e_01_01_02
2026_mth_3r_1e_01_01_04
2026_mth_3r_1e_01_01_05
2026_mth_3r_1e_01_02_01
2026_mth_3r_1e_01_02_02
2026_mth_3r_1e_01_02_03
2026_mth_3r_1e_01_03_01
2026_mth_3r_1e_01_03_02
2026_mth_3r_1e_01_03_03
2026_mth_3r_1e_01_03_04
2026_mth_3r_1e_01_03_05
2026_mth_3r_1e_01_03_06
2026_mth_3r_1e_01_04_01
2026_mth_3r_1e_01_04_02
2026_mth_3r_1e_01_04_03
2026_mth_3r_1e_01_05_01
2026_mth_3r_1e_01_05_02
2026_mth_3r_1e_01_05_03
2026_mth_3r_1e_01_06_01
2026_mth_3r_1e_01_06_02
2026_sci_4r_1e_01_01_01
2026_sci_4r_1e_01_01_02
2026_sci_4r_1e_01_01_03
2026_sci_4r_1e_01_01_04
2026_sci_4r_1e_01_01_05
2026_sci_4r_1e_01_01_06
2026_sci_4r_1e_01_02_01
2026_sci_4r_1e_01_02_04
2026_sci_4r_1e_01_02_05
2026_sci_4r_1e_01_02_02
2026_sci_4r_1e_01_02_03
2026_sci_4r_1e_01_02_06
2026_sci_4r_1e_01_03_01
2026_sci_4r_1e_01_03_02
2026_sci_4r_1e_01_03_03
2026_sci_4r_1e_01_03_06
2026_sci_4r_1e_01_04_01
2026_sci_4r_1e_01_04_02
2026_sci_4r_1e_01_04_03
2026_sci_4r_1e_01_04_06
2026_sci_4r_1e_01_05_01
2026_sci_4r_1e_01_05_02
2026_sci_4r_1e_01_05_03
2026_sci_4r_1e_01_05_06
2026_sci_4r_1e_01_06_01
2026_sci_4r_1e_01_06_02
';

DECLARE @LoNames TABLE (Name NVARCHAR(400) NOT NULL);

INSERT INTO @LoNames (Name)
SELECT DISTINCT LTRIM(RTRIM(value))
FROM STRING_SPLIT(REPLACE(@LoList, CHAR(13), N''), CHAR(10))
WHERE LTRIM(RTRIM(value)) <> N'';

;WITH NamedLos AS (
    SELECT
        lo.Id,
        lo.Name,
        lo.CreateAt,
        lo.DoneAt,
        REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '') AS NormalizedName
    FROM LearningObjectives lo
    INNER JOIN (
        SELECT DISTINCT REPLACE(LOWER(LTRIM(RTRIM(Name))), ' ', '') AS NormalizedName
        FROM @LoNames
    ) n ON REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '') = n.NormalizedName
    WHERE lo.Archived = 0
),
WithIncomplete AS (
    SELECT
        n.Id,
        n.Name,
        n.CreateAt,
        n.DoneAt,
        n.NormalizedName,
        (
            SELECT COUNT(*)
            FROM Tasks t
            WHERE t.LearningObjectiveId = n.Id
              AND t.Archived = 0
              AND t.Status <> 3
        ) AS IncompleteTaskCount
    FROM NamedLos n
),
WithRepeat AS (
    SELECT
        w.*,
        COUNT(*) OVER (PARTITION BY w.NormalizedName) AS RepeatCount,
        MAX(CASE WHEN w.IncompleteTaskCount > 0 THEN 1 ELSE 0 END)
            OVER (PARTITION BY w.NormalizedName) AS NameHasIncomplete
    FROM WithIncomplete w
)
SELECT
    Id,
    Name,
    CreateAt,
    DoneAt,
    IncompleteTaskCount,
    RepeatCount,
    CASE WHEN RepeatCount > 1 THEN 1 ELSE 0 END AS Repeated
FROM WithRepeat
WHERE NameHasIncomplete = 1
ORDER BY
    Repeated DESC,
    NormalizedName,
    CreateAt DESC,
    Id DESC;
