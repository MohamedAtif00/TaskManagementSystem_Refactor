using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Services.Email
{
    public static class EmailTemplate
    {
        public static string CreateTemplate(string fullName, string email, string startDate, string endDate, string leaveType, string reason)
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
                        <h2>طلب أجازة</h2>
                        <div class='section'>
                            <span class='label'>الموظف:</span> {fullName}
                        </div>
                        <div class='section'>
                            <span class='label'>البريد الإلكتروني:</span> {email}
                        </div>
                        <div class='section'>
                            <span class='label'>تاريخ البدء:</span> {startDate:yyyy-MM-dd}
                        </div>
                        <div class='section'>
                            <span class='label'>تاريخ الانتهاء:</span> {endDate:yyyy-MM-dd}
                        </div>
                        <div class='section'>
                            <span class='label'>نوع الأجازة:</span> {leaveType}
                        </div>
                        <div class='section'>
                            <span class='label'>سبب الأجازة:</span>
                            <div style='margin-top:5px'>{reason}</div>
                        </div>
                    </div>
                </body>
                </html>";
    }
}
