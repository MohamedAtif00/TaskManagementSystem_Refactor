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
                        *{{
                            font-size: 10px;

                        }}
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
                        <h3>Digital<h3>
                        <h4>طلب أجازة</h4>
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
                            <span class='label'>نوع الأجازة:</span> {(leaveType == LeaveRequestType.Sick?"مرضى":leaveType == LeaveRequestType.Emergency?"عارضة":leaveType == LeaveRequestType.FromNextBalance?"من الرصيد القادم":"إعتيادي")}
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
*{{
                            font-size: 10px;

                        }}
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
                                <h3>Digital<h3>
                                <h4>  طلب الإذن</h4>
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
*{{
                            font-size: 10px;

                        }}
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
                        <h3>Digital<h3>
                        <h4> إلغاء طلب الأجازة</h4>
                        <div class='section'><span class='label'>الموظف:</span> {fullName}</div>
                        <div class='section'><span class='label'>تاريخ البدء:</span> {startDate}</div>
                        <div class='section'><span class='label'>تاريخ الانتهاء:</span> {endDate}</div>
                        <div class='section'><span class='label'>مدة الأجازة:</span> {duration} يوم</div>
                        <div class='section'><span class='label'>نوع الأجازة:</span> {(leaveType == LeaveRequestType.Sick ? "مرضى" : leaveType == LeaveRequestType.Emergency ? "عارضة" : "سنوية")}</div>
                        <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                    </div>
                </body>
                </html>";


        public static string CreatePermissionCancellationTemplate(
                     string fullName, string email, string permissionDate, string from, string to, PermissionType type,string hrCode) => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
*{{
                            font-size: 10px;

                        }}
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
                        <h3>Digital<h3>
                        <h4> إلغاء طلب الإذن</h4>
                        <div class='section'><span class='label'>الموظف:</span> {fullName}</div>
                        <div class='section'><span class='label'>تاريخ الإذن:</span> {permissionDate}</div>
                        <div class='section'><span class='label'>من الساعة:</span> {from}</div>
                        <div class='section'><span class='label'>إلى الساعة:</span> {to}</div>
                        <div class='section'><span class='label'>نوع الإذن:</span> {TranslatePermissionType(type)}</div>
                        <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                    </div>
                </body>
                </html>";

        // New WorkFromHome Template for approval/creation
        public static string CreateWorkFromHomeApprovedTemplate(string fullName, string email, string date, string? noteForManager, string hrCode) => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
*{{
                            font-size: 10px;

                        }}
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
                        <h3>Digital<h3>
                        <h4>طلب عمل من المنزل</h4>
                        <div class='section'>
                            <span class='label'>الموظف:</span> {fullName}
                        </div>
                        <div class='section'>
                            <span class='label'>تاريخ العمل من المنزل:</span> {date:yyyy-MM-dd}
                        </div>
                        <div class='section'>
                            <span class='label'>عدد ساعات العمل من المنزل:</span> 8
                        </div>
                        <div class='section'>
                          <div class='section'><span class='label'>كود الموظف:</span> {hrCode}</div>
                        </div>
                    </div>
                </body>
                </html>";

        // New WorkFromHome Template for cancellation
        public static string CreateWorkFromHomeCancellationTemplate(string fullName, string email, string date, string hrCode) => $@"
                <html lang='ar' dir='rtl'>
                <head>
                    <style>
                        *{{
                            font-size: 10px;

                        }}
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
                        <h3>Digital<h3>
                        <h4> إلغاء طلب العمل من المنزل</h4>
                        <div class='section'><span class='label'>الموظف:</span> {fullName}</div>
                        <div class='section'><span class='label'>تاريخ العمل من المنزل:</span> {date}</div>
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
                PermissionType.LateArrival => "تأخير",
                PermissionType.Departure => "انصراف",
                _ => "غير معروف"
            };
        }

    }
}

