namespace MediaQR.Domain.Entities;

public enum AnalyticsEventType
{
    Scan = 1,

    Play = 2,
    Pause = 3,

    Progress25 = 4,
    Progress50 = 5,
    Progress75 = 6,
    Progress100 = 7,

    Complete = 8
}