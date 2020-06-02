using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using System.Windows.Forms;

namespace GTA_Passanger_Mod
{
    class Menu : Script
    {
        Main main_menu;

        MenuPool _menuPool;
        public Menu()
        {
            if (main_menu == null)
            {
                main_menu = new Main();
            }

            _menuPool = new MenuPool();
            var mainMenu = new UIMenu(main_menu.Mod_Name, "~b~Take a lift");
            _menuPool.Add(mainMenu);
            AddModMenu(mainMenu);
            
            _menuPool.RefreshIndex();

            Tick += (o, e) => _menuPool.ProcessMenus();
            KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.F9 && !_menuPool.IsAnyMenuOpen()) // Our menu on/off switch
                    mainMenu.Visible = !mainMenu.Visible;
            };
        }

        public void AddModMenu(UIMenu menu)
        {
            var modactivecheckbox = new UIMenuCheckboxItem("Activate Mod?", Main.Mod_Active, "Do you wish to activate the mod?");
            menu.AddItem(modactivecheckbox);

            var tipactivecheckbox = new UIMenuCheckboxItem("Give Tip?", Main.Mod_Active, "Do you wish to tip your riders for the ride?");
            menu.AddItem(tipactivecheckbox);

            menu.OnCheckboxChange += (sender, item, checked_) =>
            {
                if (item == modactivecheckbox)
                {
                    Main.Mod_Active = checked_;
                    UI.Notify("~r~" + main_menu.Mod_Name +" : ~b~" + Main.Mod_Active);
                }

                if (item == tipactivecheckbox)
                {
                    Main.Give_Tip = checked_;
                    UI.Notify("Auto Tip : ~b~" + Main.Give_Tip);
                }
            };
        }
    }
}
