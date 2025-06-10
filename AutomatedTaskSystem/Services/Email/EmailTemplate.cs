using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.Email
{
    public static class EmailTemplate
    {
        public static string CreateTemplate(string fullName,
                                            string email,
                                            string startDate,
                                            string endDate,
                                            int duration,
                                            LeaveRequestType leaveType,
                                            string hrCode)
            => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background-color: #f9f9f9;
                            color: #333;
                            padding: 20px;
                        }}
                        .container {{
                            background-color: #ffffff;
                            padding: 20px;
                            border: 1px solid #ddd;
                            border-radius: 8px;
                            max-width: 600px;
                            margin: auto;
                        }}
                        h2 {{
                            color: #2E86C1;
                        }}
                        .section {{
                            margin-bottom: 15px;
                        }}
                        .label {{
                            font-weight: bold;
                            display: inline-block;
                            min-width: 100px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Digital<h1>
                        <h2>طلب أجازة</h2>
                        <div class='section'>
                            <span class='label'>الموظف:</span> {fullName}
                        </div>
                        <div class='section'>
                            <span class='label'>تاريخ البدء:</span> {startDate:yyyy-MM-dd}
                        </div>
                        <div class='section'>
                            <span class='label'>تاريخ الانتهاء:</span> {endDate:yyyy-MM-dd}
                        </div>
                        <div class='section'>
                            <span class='label'> مدة الأجازة:</span> {duration}
                        </div>
                        <div class='section'>
                            <span class='label'>نوع الأجازة:</span> {(leaveType == LeaveRequestType.Sick?"مرضي":leaveType == LeaveRequestType.Emergency?"عرضة":"سنويا")}
                        </div>
                        <div class='section'>
                            <span class='label'>كود الموظف:</span>
                            <div style='margin-top:5px'>{hrCode}</div>
                        </div>
                    </div>
                </body>
                </html>";


        public static string CreatePermissionTemplate(
                       string fullName,
                       string email,
                       string permissionDate,
                       string from,
                       string to,
                        PermissionType type,
                        string hrCode)
                       => $@"
                        <html lang='ar' dir='rtl'>
                        <head>
                            <style>
                                body {{
                                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                    background-color: #f9f9f9;
                                    color: #333;
                                    padding: 20px;
                                }}
                                .container {{
                                    background-color: #ffffff;
                                    padding: 20px;
                                    border: 1px solid #ddd;
                                    border-radius: 8px;
                                    max-width: 600px;
                                    margin: auto;
                                }}
                                h2 {{
                                    color: #2E86C1;
                                }}
                                .section {{
                                    margin-bottom: 15px;
                                }}
                                .label {{
                                    font-weight: bold;
                                    display: inline-block;
                                    min-width: 100px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h1>Digital<h1>
                                <h2>  طلب الإذن</h2>
                                <div class='section'>
                                    <span class='label'>الموظف:</span> {fullName}
                                </div>
                                <div class='section'>
                                    <span class='label'>تاريخ الإذن:</span> {permissionDate}
                                </div>
                                <div class='section'>
                                    <span class='label'>من الساعة:</span> {from}
                                </div>
                                <div class='section'>
                                    <span class='label'>إلى الساعة:</span> {to}
                                </div>
                                <div class='section'>
                                   <span class='label'>نوع الإذن:</span> {TranslatePermissionType(type)}
                                </div>
                                 <div class='section'>
                                   <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                                </div>
                            </div>
                        </body>
                        </html>";
        public static string CreateLeaveCancellationTemplate(
                     string fullName, string email, string startDate, string endDate,
                     int duration, LeaveRequestType leaveType, string hrCode) => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background-color: #f9f9f9;
                            color: #333;
                            padding: 20px;
                        }}
                        .container {{
                            background-color: #fff;
                            padding: 20px;
                            border: 1px solid #ddd;
                            border-radius: 8px;
                            max-width: 600px;
                            margin: auto;
                        }}
                        h2 {{ color: #C0392B; }}
                        .section {{ margin-bottom: 15px; }}
                        .label {{
                            font-weight: bold;
                            display: inline-block;
                            min-width: 100px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Digital<h1>
                        <h2> إلغاء طلب الأجازة</h2>
                        <div class='section'><span class='label'>الموظف:</span> {fullName}</div>
                        <div class='section'><span class='label'>تاريخ البدء:</span> {startDate}</div>
                        <div class='section'><span class='label'>تاريخ الانتهاء:</span> {endDate}</div>
                        <div class='section'><span class='label'>مدة الأجازة:</span> {duration} يوم</div>
                        <div class='section'><span class='label'>نوع الأجازة:</span> {(leaveType == LeaveRequestType.Sick ? "مرضي" : leaveType == LeaveRequestType.Emergency ? "عرضة" : "سنوية")}</div>
                        <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                    </div>
                </body>
                </html>";


        public static string CreatePermissionCancellationTemplate(
                     string fullName, string email, string permissionDate, string from, string to, PermissionType type,string hrCode) => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background-color: #f9f9f9;
                            color: #333;
                            padding: 20px;
                        }}
                        .container {{
                            background-color: #fff;
                            padding: 20px;
                            border: 1px solid #ddd;
                            border-radius: 8px;
                            max-width: 600px;
                            margin: auto;
                        }}
                        h2 {{ color: #C0392B; }}
                        .section {{ margin-bottom: 15px; }}
                        .label {{
                            font-weight: bold;
                            display: inline-block;
                            min-width: 100px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Digital<h1>
                        <h2> إلغاء طلب الإذن</h2>
                        <div class='section'><span class='label'>الموظف:</span> {fullName}</div>
                        <div class='section'><span class='label'>تاريخ الإذن:</span> {permissionDate}</div>
                        <div class='section'><span class='label'>من الساعة:</span> {from}</div>
                        <div class='section'><span class='label'>إلى الساعة:</span> {to}</div>
                        <div class='section'><span class='label'>نوع الإذن:</span> {TranslatePermissionType(type)}</div>
                        <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                    </div>
                </body>
                </html>";



        public static string TranslatePermissionType(PermissionType type)
        {
            return type switch
            {
                PermissionType.WorkAssignment => "مهمة عمل",
                PermissionType.EarlyDeparture => "انصراف مبكر",
                PermissionType.LateArrival => "حضور متأخر",
                PermissionType.Departure => "انصراف",
                _ => "غير معروف"
            };
        }

    }
}

