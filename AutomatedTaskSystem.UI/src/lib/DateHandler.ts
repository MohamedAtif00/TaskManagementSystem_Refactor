const dateHandler = (params: string) => {
    const date = new Date(params);

    const MonthNames = new Map<number, string>();

    MonthNames.set(0, "Jan");
    MonthNames.set(1, "Feb");
    MonthNames.set(2, "Mar");
    MonthNames.set(3, "Apr");
    MonthNames.set(4, "May");
    MonthNames.set(5, "Jun");
    MonthNames.set(6, "Jul");
    MonthNames.set(7, "Aug");
    MonthNames.set(8, "Sep");
    MonthNames.set(9, "Oct");
    MonthNames.set(10, "Nov");
    MonthNames.set(11, "Dec");

    const dateHours = date.getHours() % 12 === 0 ? 12 : date.getHours() % 12;

    const dateMinutes =
        date.getMinutes() < 10
            ? `0${date.getMinutes()}`
            : `${date.getMinutes()}`;

    const time = {
        date: `${date.getDate()} ${MonthNames.get(
            date.getMonth()
        )} ${date.getFullYear()}`,
        hours: dateHours < 10 ? `0${dateHours}` : `${dateHours}`,
        minutes: dateMinutes,
        con: date.getHours() > 12 ? "PM" : "AM",
    };

    return time;
};

export default dateHandler;
