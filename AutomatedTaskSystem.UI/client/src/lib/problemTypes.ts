export const PROBLEM_TYPES = [
	"Content",
	"Logic",
	"UI",
	"API",
	"Performance",
	"Development",
	"VO/Narration",
	"Others",
] as const;

export type ProblemType = (typeof PROBLEM_TYPES)[number];
