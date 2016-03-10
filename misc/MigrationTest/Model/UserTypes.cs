namespace Model
{
    public enum UserStatus : byte
    {
        Normal = 0,
        Unregistered = 1,
        Banned = 2,
    }

    public enum UserPermission : byte
    {
        Normal = 0,
        Tester = 2,
        Administrator = 3,
        Developer = 4,
        TestBot = 5,
    }
}
