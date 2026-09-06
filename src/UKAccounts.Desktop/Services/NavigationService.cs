namespace UKAccounts.Desktop.Services;

public interface INavigationService
{
    void NavigateTo(string viewKey);
}

public class NavigationService : INavigationService
{
    public void NavigateTo(string viewKey)
    {
    }
}
