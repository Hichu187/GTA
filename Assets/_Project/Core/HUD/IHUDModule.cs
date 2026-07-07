namespace Game.Core.HUD
{
    public interface IHUDModule
    {
        // dataSource is typed by each concrete module (e.g. IVehicleSpeedSource).
        void Bind(object dataSource);
        void Show();
        void Hide();
    }
}
