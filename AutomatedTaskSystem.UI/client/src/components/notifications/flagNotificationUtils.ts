export interface FlagNotificationData {
    projectId?: number;
    taskName?: string;
    learningObjectiveName?: string;
    projectName?: string;
    flaggedByUserName?: string;
    comment?: string;
    canView?: boolean;
}

export const isFlagNotification = (
    title: string,
    additionalData?: string | null
): boolean => {
    if (title === "Task Flagged") return true;

    if (additionalData) {
        try {
            const data = JSON.parse(additionalData);
            return data.kind === "flagged";
        } catch {
            return false;
        }
    }

    return false;
};

export const parseFlagNotificationData = (notification: {
    title: string;
    message: string;
    additionalData?: string | null;
}): FlagNotificationData => {
    if (notification.additionalData) {
        try {
            const data = JSON.parse(notification.additionalData);
            if (data.kind === "flagged") {
                return {
                    projectId: data.projectId,
                    taskName: data.taskName,
                    learningObjectiveName: data.learningObjectiveName,
                    projectName: data.projectName,
                    flaggedByUserName: data.flaggedByUserName,
                    comment: data.comment,
                    canView: data.canView === true,
                };
            }
        } catch {
            // fall through to message parsing
        }
    }

    const match = notification.message.match(
        /^(.+?) flagged task '(.+?)' in LO '(.+?)' \(project '(.+?)'\)\. Comment: (.+)$/s
    );

    if (match) {
        return {
            flaggedByUserName: match[1],
            taskName: match[2],
            learningObjectiveName: match[3],
            projectName: match[4],
            comment: match[5],
        };
    }

    const legacyMatch = notification.message.match(
        /^(.+?) flagged task '(.+?)' in project '(.+?)'\. Comment: (.+)$/s
    );

    if (legacyMatch) {
        return {
            flaggedByUserName: legacyMatch[1],
            taskName: legacyMatch[2],
            projectName: legacyMatch[3],
            comment: legacyMatch[4],
        };
    }

    return { comment: notification.message };
};
