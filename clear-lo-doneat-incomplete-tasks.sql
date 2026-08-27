-- Clears LearningObjectives.DoneAt when the LO is in the provided name list
-- and still has at least one non-archived incomplete task.
--
-- App rule (TaskService.UpdateLearningObjectiveCompletionAsync):
--   DoneAt is set only when every non-archived task is Done (Status = 3).
--   Otherwise DoneAt must be NULL.
--
-- TaskStatusEnum: 0=Backlog, 1=ToDo, 2=Doing, 3=Done, 4=Rollback
-- Incomplete = Status <> 3 (not only Status = 0).
--
-- Matching is case-insensitive and ignores spaces so names like
-- 'mth_1r_1a _01_04_03' still match the DB row.
-- Archived LOs are excluded and never updated.
-- If several non-archived LOs share the same name, only the latest
-- CreateAt is used (Id DESC as a tie-breaker).
--
-- Paste or add LO names in @LoList below, one per line.
-- Review the preview result sets, then COMMIT (or ROLLBACK).

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @LoList NVARCHAR(MAX) = N'
2025_ara_2r_1a_01_03_01
2025_ara_2r_1a_01_01_02
2025_ara_2r_1a_01_01_03
2025_ara_2r_1a_01_02_01
2025_ara_2r_1a_01_02_04
2025_ara_2r_1a_01_02_02
2025_ara_2r_1a_01_02_03
2025_ara_3r_1a_01_02_01
2025_ara_3r_1a_01_03_01
2025_ara_2r_1a_01_03_02
Ara_1R_2A_01_03_03
Ara_1R_2A_02_02_03
Ara_1R_2A_02_04_03
Ara_1R_2A_03_02_03
Ara_1R_2A_04_04_01
2025_ara_3r_1a_01_06_01
2026_ara_2r_1a_01_05_03
2025_ara_2r_1a_01_06_01
2025_ara_2r_1a_01_06_04
2025_ara_3r_1a_01_07_01
2025_ara_2r_1a_01_06_02
2025_ara_2r_1a_01_06_03
2025_ara_2r_1a_01_07_01
2025_eng_4r_1e_01_05_02
2025_ara_2r_1a_01_07_02
2025_soc_4r_1a_01_04_01
2025_ara_2r_1a_01_08_04
2025_ara_2r_1a_01_08_02
2025_ara_2r_1a_01_08_03
2025_soc_4r_1a_01_05_01
2026_ara_2r_1a_01_01_01
2025_ara_2r_1a_01_09_01
2025_ara_2r_1a_01_09_02
QR_2025_ara_3r_1a_01_01_01
2026_ara_3r_1a_01_01_01
2025_ara_3r_1a_01_01_02
2025_ara_3r_1a_01_01_03
2026_ara_2r_1a_01_03_01
2025_ara_3r_1a_01_02_04
2026_ara_3r_1a_01_02_02
2025_ara_3r_1a_01_02_02
2025_ara_3r_1a_01_02_03
2026_ara_2r_1a_01_05_02
2026_ara_2r_1a_01_06_02
2025_ara_3r_1a_01_03_02
2026_ara_3r_1a_01_04_01
2026_ara_3r_1a_01_04_02
2026_ara_2r_1a_01_08_01
2026_ara_3r_1a_01_05_02
2026_ara_2r_1a_01_09_01
2025_ara_3r_1a_01_06_04
2026_ara_2r_1a_01_09_02
2025_ara_3r_1a_01_06_02
2025_ara_3r_1a_01_06_03
2026_ara_3r_1a_01_03_01
2026_ara_3r_1a_01_05_01
2025_ara_3r_1a_01_07_02
2026_ara_3r_1a_01_06_02
2025_ara_3r_1a_01_08_06
2025_ara_3r_1a_01_08_02
2025_ara_3r_1a_01_08_03
2026_ara_3r_1a_01_07_01
2026_ara_3r_1a_01_08_01
2025_ara_3r_1a_01_09_01
2025_ara_3r_1a_01_09_02
QR_2025_ara_4r_1a_01_01_01
2026_ara_3r_1a_01_09_01
2025_ara_4r_1a_01_01_02
2025_ara_4r_1a_01_01_03
2026_ara_3r_1a_01_09_02
2025_ara_4r_1a_01_02_04
2025_ara_4r_1a_01_02_02
2025_ara_4r_1a_01_02_03
2026_ara_4r_1a_01_01_01
2026_ara_4r_1a_01_02_03
2025_ara_4r_1a_01_03_01
2025_ara_4r_1a_01_03_02
Ara_4R_1A_01_05_01
Ara_4R_1A_01_05_02
2025_ara_4r_1a_01_04_02
2026_ara_4r_1a_01_02_01
2026_ara_4r_1a_01_02_02
2026_ara_4r_1a_01_04_01
2026_ara_4r_1a_01_05_02
2025_ara_4r_1a_01_06_04
2025_ara_4r_1a_01_06_02
2025_ara_4r_1a_01_06_03
2026_ara_4r_1a_01_05_03
2026_ara_4r_1a_01_06_01
2026_ara_4r_1a_01_06_02
Ara_4R_1A_01_05_03
Ara_4R_1A_01_05_04
Ara_4R_1A_02_05_01
Ara_4R_1A_02_05_02
Ara_4R_1A_02_05_03
Ara_4R_1A_02_05_04
2026_ara_4r_1a_01_06_03
Ara_4R_1A_02_05_06
2026_ara_4r_1a_01_08_01
Ara_4R_1A_01_02_04
2025_ara_4r_1a_01_08_01
2025_ara_4r_1a_01_08_02
2026_ara_4r_1a_01_09_01
2026_ara_4r_1a_01_09_02
2025_ara_4r_1a_01_09_01
QR_2025_ara_5r_1a_01_01_01
2026_ara_5r_1a_01_01_01
2025_ara_5r_1a_01_01_02
2025_ara_5r_1a_01_01_03
2026_ara_5r_1a_01_02_01
2025_ara_5r_1a_01_02_04
2025_ara_5r_1a_01_02_02
2025_ara_5r_1a_01_02_03
2026_ara_5r_1a_01_02_02
2026_ara_5r_1a_01_02_03
2026_ara_5r_1a_01_03_01
Ara_5R_1A_02_06_02
2025_ara_5r_1a_01_04_02
2026_ara_5r_1a_01_04_01
2026_ara_5r_1a_01_04_02
2026_ara_5r_1a_01_05_02
2026_ara_5r_1a_01_05_03
2026_ara_5r_1a_01_06_01
2025_ara_5r_1a_01_06_04
2025_ara_5r_1a_01_06_02
2025_ara_5r_1a_01_06_03
2025_ara_5r_1a_01_07_01
2026_ara_5r_1a_01_07_01
2025_ara_5r_1a_01_07_02
2026_ara_5r_1a_01_08_01
2025_ara_5r_1a_01_08_04
2025_ara_5r_1a_01_08_02
2025_ara_5r_1a_01_08_03
2026_ara_5r_1a_01_09_01
2026_ara_5r_1a_01_09_02
2025_ara_5r_1a_01_09_01
QR_2025_ara_6r_1a_01_01_01
2026_ara_6r_1a_01_01_01
2025_ara_6r_1a_01_01_02
2025_ara_6r_1a_01_01_03
2026_ara_6r_1a_01_02_01
2025_ara_6r_1a_01_02_05
2025_ara_6r_1a_01_02_02
2025_ara_6r_1a_01_02_03
2026_ara_6r_1a_01_02_02
2026_ara_6r_1a_01_06_01
2026_ara_6r_1a_01_08_01
2026_ara_6r_1a_01_03_01
ara_6r_1a_01_06_03
2025_ara_6r_1a_01_04_02
2026_ara_6r_1a_01_04_02
2026_ara_6r_1a_01_04_03
2026_ara_6r_1a_01_05_02
2026_ara_6r_1a_01_05_03
2026_ara_6r_1a_01_08_04
2025_ara_6r_1a_01_06_05
2025_ara_6r_1a_01_06_02
2025_ara_6r_1a_01_06_03
ara_6r_2a_03_02_01
2026_ara_6r_1a_01_07_01
ara_6r_2a_03_02_03
2026_ara_6r_1a_01_09_01
2025_ara_6r_1a_01_08_05
2025_ara_6r_1a_01_08_02
2025_ara_6r_1a_01_08_04
2026_ara_6r_1a_01_09_02
2026_eng_1r_1e_02_01_04
2026_eng_1r_1e_02_01_05
2025_ara_6r_1a_01_09_01
2026_eng_1r_1e_01_01_02
2025_eng_1r_1e_02_01_02
2025_eng_1r_1e_02_01_01
2026_eng_1r_1e_02_02_01
2026_eng_1r_1e_02_02_03
2026_eng_1r_1e_02_03_03
2026_eng_1r_1e_02_04_01
2025_eng_1r_1e_02_02_02
2025_eng_1r_1e_02_03_01
2025_eng_1r_1e_02_03_03
2026_eng_1r_1e_02_04_03
2026_eng_1r_1e_02_05_01
2026_eng_1r_1e_02_05_02
2025_eng_1r_1e_02_04_02
2026_eng_2r_1e_01_02_02
2026_eng_2r_1e_01_02_03
2025_eng_2r_1e_01_01_01
2025_eng_2r_1e_01_01_03
2025_eng_2r_1e_01_01_05
2026_eng_2r_1e_01_01_04
2026_eng_2r_1e_01_02_04
2026_eng_2r_1e_01_03_03
2025_eng_2r_1e_01_01_07
2026_eng_2r_1e_01_03_04
2025_eng_2r_1e_01_02_01
2025_eng_2r_1e_01_02_03
2026_eng_2r_1e_01_04_01
2025_eng_2r_1e_01_02_05
2026_eng_2r_1e_01_05_01
2026_eng_2r_1e_01_05_02
2026_eng_3r_1e_01_01_03
2026_eng_3r_1e_01_01_04
2026_eng_3r_1e_01_01_01
2025_eng_3r_1e_01_01_02
2025_eng_3r_1e_01_01_03
2026_eng_3r_1e_01_02_02
2026_eng_3r_1e_01_02_03
2025_eng_3r_1e_01_02_02
2026_eng_3r_1e_01_03_04
2025_eng_3r_1e_01_02_04
2026_eng_3r_1e_01_03_05
2025_eng_3r_1e_01_03_01
2025_eng_3r_1e_01_03_02
2026_eng_3r_1e_01_04_03
2025_eng_3r_1e_01_03_04
2026_eng_3r_1e_01_04_04
2025_eng_3r_1e_01_04_01
2025_eng_3r_1e_01_04_02
2026_eng_3r_1e_01_05_01
2025_eng_3r_1e_01_04_04
2026_eng_3r_1e_01_05_02
2026_eng_4r_1e_01_01_04
2026_eng_4r_1e_01_03_01
2026_eng_4r_1e_01_01_02
2025_eng_4r_1e_01_01_01
2025_eng_4r_1e_01_01_03
2025_eng_4r_1e_01_01_05
2026_eng_4r_1e_01_03_03
2025_eng_4r_1e_01_02_01
2026_eng_4r_1e_01_03_05
eng_5r_1e_06_02_05
2025_eng_4r_1e_01_02_02
2026_eng_4r_1e_01_02_02
2026_eng_4r_1e_01_05_01
2025_eng_4r_1e_01_03_04
2026_eng_4r_1e_01_05_02
2025_eng_4r_1e_01_03_06
2026_eng_5r_1e_01_01_02
2025_eng_4r_1e_01_04_01
2026_eng_5r_1e_01_02_02
2026_eng_5r_1e_01_03_01
2026_eng_5r_1e_01_04_02
2025_eng_5r_1e_01_01_01
2025_eng_5r_1e_01_01_02
2025_eng_5r_1e_01_01_03
2026_eng_5r_1e_01_05_01
2025_eng_5r_1e_01_02_01
2026_eng_5r_1e_01_05_02
eng_5r_1e_07_02_02
2025_eng_5r_1e_01_02_02
2026_eng_5r_1e_01_02_03
2026_eng_6r_1e_01_01_02
2025_eng_5r_1e_01_03_04
2026_eng_5r_1e_01_03_04
2025_eng_5r_1e_01_04_01
2026_eng_6r_1e_01_01_04
2026_eng_6r_1e_01_02_02
2025_eng_5r_1e_01_05_02
2026_eng_6r_1e_01_02_03
2026_eng_6r_1e_01_01_03
2025_eng_6r_1e_01_01_02
2026_eng_6r_1e_01_03_03
2025_eng_6r_1e_01_02_01
2026_eng_6r_1e_01_04_02
2026_eng_6r_1e_01_05_01
2025_eng_6r_1e_01_02_03
2026_eng_6r_1e_01_03_02
2026_eng_6r_1e_01_05_02
2025_eng_6r_1e_01_03_02
2025_eng_6r_1e_01_03_04
2026_eng_6r_1e_01_04_01
2025_eng_6r_1e_01_04_01
2026_mth_1r_1a_01_01_01
2025_eng_6r_1e_01_04_02
2026_mth_1r_1a_01_01_02
2026_mth_1r_1a_01_02_01
2025_eng_6r_1e_01_05_02
2026_mth_1r_1a_01_02_02
2026_mth_1r_1a_01_03_01
2026_mth_1r_1a_01_04_01
2026_mth_1r_1a_01_04_02
mth_1r_1a_01_02_02
2026_mth_1r_1a_01_05_01
QR_mth_1r_1a_01_02_03
mth_1r_1a _01_04_03
mth_1r_1a _01_04_05
mth_1r_1a _01_04_06
2026_mth_1r_1a_01_06_01
2026_mth_1r_1a_01_06_02
2025_mth_1r_1a_01_03_02
2026_mth_1r_1a_01_07_01
mth_1r_1a _02_02_08
QR_mth_1r_1a_02_02_06
mth_1r_1a _02_02_09
2026_mth_1r_1a_01_07_02
2026_mth_1r_1a_01_08_01
2026_mth_1r_1a_01_08_02
2026_mth_1r_1a_01_09_01
2025_mth_1r_1a_01_05_02
2026_mth_1r_1a_01_09_02
2026_mth_1r_1a_01_10_01
2025_mth_1r_1a_01_05_04
2026_mth_1r_1a_01_10_02
2026_mth_1r_1a_01_11_01
2025_mth_1r_1a_01_05_06
2026_mth_1r_1a_01_13_01
2026_mth_1r_1a_01_14_02
2025_mth_1r_1a_01_06_02
2026_mth_1r_1e_01_14_01
2025_mth_1r_1a_01_06_04
QR_mth_1r_2a_04_04_03
mth_1r_1a_03_01_07
mth_1r_1a_03_01_08
mth_1r_1a_03_01_09
mth_1r_1a_03_01_10
mth_1r_1a_03_01_013
mth_1r_1a_03_01_014
2026_mth_1r_1e_01_14_02
mth_1r_1a_01_04_02
2025_mth_1r_1a_01_08_02
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
2026_mth_3r_1a_01_01_01
2026_mth_3r_1a_01_01_02
2026_mth_3r_1a_01_01_04
2026_mth_3r_1a_01_01_05
2025_soc_4r_1a_01_01_02
2026_soc_4r_1a_01_01_05
2025_soc_4r_1a_01_01_03
2026_soc_4r_1a_01_01_06
2025_soc_4r_1a_01_01_04
2025_soc_4r_1a_01_02_01
2026_soc_4r_1a_01_02_01
2026_soc_4r_1a_01_02_03
2026_mth_3r_1a_01_02_01
2025_soc_4r_1a_01_02_03
2025_soc_4r_1a_01_03_02
2026_soc_4r_1a_01_03_01
Soc_4R_1A_01_04_03_p2
Soc_4R_1A_01_01_03
2025_soc_4r_1a_01_03_04
2026_mth_3r_1a_01_02_02
2026_mth_3r_1a_01_02_03
2026_soc_4r_1a_01_04_03
Soc_4R_1A_01_02_05
2026_soc_4r_1a_01_04_04
2025_soc_4r_1a_01_04_03
2026_soc_4r_1a_01_05_01
2026_mth_3r_1a_01_03_01
2026_mth_3r_1a_01_03_02
2025_soc_4r_1a_01_05_02
2025_soc_5r_1a_01_01_01
2026_soc_5r_1a_01_01_01
2025_soc_5r_1a_01_01_02
2026_soc_5r_1a_01_01_02
2025_soc_5r_1a_01_01_03
2026_mth_3r_1a_01_03_04
2025_soc_5r_1a_01_01_06
2025_soc_5r_1a_01_01_05
2025_soc_5r_1a_01_01_07
2025_soc_5r_1a_01_02_01
2026_soc_5r_1a_01_02_03
2025_soc_5r_1a_01_02_02
2026_soc_5r_1a_01_02_04
2025_soc_5r_1a_01_02_03
2026_mth_3r_1a_01_03_06
2026_mth_3r_1a_01_04_01
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
2026_mth_3r_1a_01_04_02
2026_mth_3r_1a_01_04_03
2026_mth_3r_1a_01_05_01
2026_mth_3r_1a_01_05_02
2026_mth_3r_1a_01_05_03
2026_mth_3r_1a_01_06_01
2026_mth_3r_1a_01_06_02
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
2026_sci_4r_1e_01_02_02
2026_sci_4r_1e_01_02_03
2026_sci_4r_1e_01_02_04
2026_sci_4r_1e_01_02_05
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
2026_soc_4r_1a_01_02_02
2026_soc_4r_1a_01_04_02
2026_soc_4r_1a_01_05_02
2026_soc_5r_1a_01_01_04
2026_soc_5r_1a_01_02_06
2026_soc_5r_1a_01_02_08
ara_3r_2a_05_03_05
Ara_4R_1A_02_05_05
Ara_5R_1A_02_06_01
ara_6r_1a_01_06_01
';

DECLARE @LoNames TABLE (Name NVARCHAR(400) NOT NULL);

INSERT INTO @LoNames (Name)
SELECT DISTINCT LTRIM(RTRIM(value))
FROM STRING_SPLIT(REPLACE(@LoList, CHAR(13), N''), CHAR(10))
WHERE LTRIM(RTRIM(value)) <> N'';

DECLARE @Matched TABLE (
    Id INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
    DoneAt DATETIME2 NULL,
    CreateAt DATETIME2 NOT NULL,
    IncompleteTaskCount INT NOT NULL
);

;WITH RankedLos AS (
    SELECT
        lo.Id,
        lo.Name,
        lo.DoneAt,
        lo.CreateAt,
        REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '') AS NormalizedName,
        ROW_NUMBER() OVER (
            PARTITION BY REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '')
            ORDER BY lo.CreateAt DESC, lo.Id DESC
        ) AS rn
    FROM LearningObjectives lo
    INNER JOIN (
        SELECT DISTINCT REPLACE(LOWER(LTRIM(RTRIM(Name))), ' ', '') AS NormalizedName
        FROM @LoNames
    ) n ON REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '') = n.NormalizedName
    WHERE lo.Archived = 0
)
INSERT INTO @Matched (Id, Name, DoneAt, CreateAt, IncompleteTaskCount)
SELECT
    r.Id,
    r.Name,
    r.DoneAt,
    r.CreateAt,
    COUNT(t.Id) AS IncompleteTaskCount
FROM RankedLos r
LEFT JOIN Tasks t
    ON t.LearningObjectiveId = r.Id
   AND t.Archived = 0
   AND t.Status <> 3
WHERE r.rn = 1
GROUP BY r.Id, r.Name, r.DoneAt, r.CreateAt;

-- Listed LOs skipped because they are archived.
SELECT
    lo.Id,
    lo.Name,
    lo.DoneAt,
    lo.Archived
FROM LearningObjectives lo
INNER JOIN @LoNames n
    ON REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '')
     = REPLACE(LOWER(LTRIM(RTRIM(n.Name))), ' ', '')
WHERE lo.Archived = 1
ORDER BY lo.Name;

-- Older duplicate names skipped (a newer CreateAt row was kept instead).
SELECT
    lo.Id,
    lo.Name,
    lo.CreateAt,
    lo.DoneAt,
    m.Id AS KeptLoId,
    m.CreateAt AS KeptCreateAt
FROM LearningObjectives lo
INNER JOIN @Matched m
    ON REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '')
     = REPLACE(LOWER(LTRIM(RTRIM(m.Name))), ' ', '')
   AND lo.Id <> m.Id
WHERE lo.Archived = 0
ORDER BY lo.Name, lo.CreateAt DESC;

-- Preview: LOs that will have DoneAt cleared.
SELECT
    Id,
    Name,
    CreateAt,
    DoneAt,
    IncompleteTaskCount
FROM @Matched
WHERE DoneAt IS NOT NULL
  AND IncompleteTaskCount > 0
ORDER BY Name;

-- Names from the list with no matching non-archived LO.
SELECT n.Name AS MissingLoName
FROM @LoNames n
WHERE NOT EXISTS (
    SELECT 1
    FROM LearningObjectives lo
    WHERE lo.Archived = 0
      AND REPLACE(LOWER(LTRIM(RTRIM(lo.Name))), ' ', '')
        = REPLACE(LOWER(LTRIM(RTRIM(n.Name))), ' ', '')
)
ORDER BY n.Name;

-- Listed LOs that already have DoneAt NULL, or have no incomplete tasks.
SELECT
    Id,
    Name,
    CreateAt,
    DoneAt,
    IncompleteTaskCount,
    CASE
        WHEN IncompleteTaskCount = 0 THEN N'all related tasks are Done (or none exist)'
        WHEN DoneAt IS NULL THEN N'already open (DoneAt is null)'
        ELSE N''
    END AS SkipReason
FROM @Matched
WHERE NOT (DoneAt IS NOT NULL AND IncompleteTaskCount > 0)
ORDER BY Name;

UPDATE lo
SET lo.DoneAt = NULL
FROM LearningObjectives lo
INNER JOIN @Matched m ON m.Id = lo.Id
WHERE lo.Archived = 0
  AND m.DoneAt IS NOT NULL
  AND m.IncompleteTaskCount > 0;

SELECT @@ROWCOUNT AS ClearedLoCount;

-- Verify updated rows.
SELECT lo.Id, lo.Name, lo.CreateAt, lo.DoneAt
FROM LearningObjectives lo
INNER JOIN @Matched m ON m.Id = lo.Id
WHERE lo.Archived = 0
  AND m.DoneAt IS NOT NULL
  AND m.IncompleteTaskCount > 0
ORDER BY lo.Name;

-- COMMIT;
ROLLBACK;
