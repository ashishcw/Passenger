using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GTA_Passanger_Mod
{
    public class Main : Script
    {

        public static bool Mod_Active = false, Give_Tip = false;

        public Ped Player_Ped;

        private List<Ped> all_peds = new List<Ped>();

        private bool vehicle_set = false;

        private int speed = 10;

        private DrivingStyle driving_style = DrivingStyle.Normal;

        private Vector3 Destionation;

        private int ridetime_started = 0;


        public readonly string Mod_Name = "Passenger Mod";
        public Main()
        {
            Tick += Main_Tick;

            KeyDown += Main_KeyDown;
        }//Default constructor ends here

        private void Set_Destination_Marker_Map()
        {
            if (Game.IsWaypointActive)
            {
                //var waypoint = World.GetWaypointPosition();
                Blip wpBlip = new Blip(Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, 8));

                if (Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE))
                {
                    GTA.Math.Vector3 wpVec = Function.Call<GTA.Math.Vector3>(Hash.GET_BLIP_COORDS, wpBlip);
                    Destionation = World.GetNextPositionOnStreet(wpVec.Around(10f));
                    GTA.UI.Notify("Destination Set");
                }
            }
            else
            {
                GTA.UI.Notify("Destination not found, driver will now cruise around");
            }
        }

        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (Mod_Active)
            {
                if (e.KeyCode == System.Windows.Forms.Keys.X)
                {
                    Player_Ped = GTA.Game.Player.Character;
                    //UI.ShowSubtitle("Mod Activated : " + Mod_Name);
                    if (Player_Ped != null)
                    {
                        if (Player_Ped.IsOnFoot)
                        {   
                            var nearest_vehicle = World.GetNearbyVehicles(Player_Ped.Position, 4f);

                            for (int i = 0; i < nearest_vehicle.Length; i++)
                            {
                                if (nearest_vehicle[i] != null)
                                {
                                    if (nearest_vehicle[i].GetPedOnSeat(VehicleSeat.Driver) != null && nearest_vehicle[i].IsSeatFree(VehicleSeat.Any))
                                    {
                                        all_peds.Add(nearest_vehicle[i].GetPedOnSeat(VehicleSeat.Driver));                                        
                                        foreach (var item in nearest_vehicle[i].Passengers.ToList())
                                        {
                                            all_peds.Add(item);
                                        }                                        
                                        
                                        foreach (var item in all_peds)
                                        {   
                                            World.SetRelationshipBetweenGroups(Relationship.Like, item.RelationshipGroup, Player_Ped.RelationshipGroup);
                                            Function.Call(GTA.Native.Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, item, 1);
                                            Function.Call(GTA.Native.Hash.SET_PED_COMBAT_ATTRIBUTES, item, 20, true);
                                        }                                        
                                        nearest_vehicle[i].GetPedOnSeat(VehicleSeat.Driver).Task.Wait(5000);
                                        UI.ShowSubtitle("Nearest Vehicle is : " + nearest_vehicle[i].DisplayName);
                                        if (nearest_vehicle[i].IsSeatFree(VehicleSeat.RightRear))
                                        {
                                            Player_Ped.Task.EnterVehicle(nearest_vehicle[i], VehicleSeat.RightRear, -1, 10f);
                                            vehicle_set = true;
                                        }
                                        else if (nearest_vehicle[i].IsSeatFree(VehicleSeat.LeftRear)) 
                                        {
                                            Player_Ped.Task.EnterVehicle(nearest_vehicle[i], VehicleSeat.LeftRear, -1, 10f);
                                            vehicle_set = true;
                                        }
                                        else if (nearest_vehicle[i].IsSeatFree(VehicleSeat.Passenger))
                                        {
                                            Player_Ped.Task.EnterVehicle(nearest_vehicle[i], VehicleSeat.Passenger, -1, 10f);
                                            vehicle_set = true;
                                        }
                                        else {
                                            UI.ShowSubtitle("No Seat Available");
                                            foreach (var item in all_peds)
                                            {
                                                item.MarkAsNoLongerNeeded();
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        if (Give_Tip && vehicle_set)
                        {
                            if (all_peds.Count > 0)
                            {
                                if (!Player_Ped.IsOnFoot)
                                {
                                    if (Player_Ped.CurrentVehicle != null)
                                    {
                                        if (Player_Ped.CurrentVehicle == all_peds[0].CurrentVehicle)
                                        {
                                            ridetime_started = Game.GameTime;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //increase speed
                if (e.KeyCode == System.Windows.Forms.Keys.Right && vehicle_set)
                {
                    if(speed < 50)
                    {
                        speed++;
                    }
                    Set_Destination_Marker_Map();
                    if(Destionation != null)
                    {
                        Player_Ped.CurrentVehicle.Driver.Task.DriveTo(Player_Ped.CurrentVehicle, Destionation, 4f, speed, (int)driving_style);
                    }
                    else
                    {
                        Player_Ped.CurrentVehicle.Driver.Task.CruiseWithVehicle(Player_Ped.CurrentVehicle, speed, (int)driving_style);
                    }

                    GTA.UI.ShowSubtitle("Speed increased to : " + speed);
                }

                //decrease speed
                if (e.KeyCode == System.Windows.Forms.Keys.Left && vehicle_set)
                {
                    if (speed > 1)
                    {
                        speed--;
                    }

                    Set_Destination_Marker_Map();
                    if (Destionation != null)
                    {
                        Player_Ped.CurrentVehicle.Driver.Task.DriveTo(Player_Ped.CurrentVehicle, Destionation, 4f, speed, (int)driving_style);
                    }
                    else
                    {
                        Player_Ped.CurrentVehicle.Driver.Task.CruiseWithVehicle(Player_Ped.CurrentVehicle, speed, (int)driving_style);
                    }
                    GTA.UI.ShowSubtitle("Speed decreased to : " + speed);
                }

                //change driving mode Normal/Rushed
                if (e.KeyCode == System.Windows.Forms.Keys.OemPeriod)
                {
                    if (driving_style == DrivingStyle.Normal)
                    {
                        driving_style = DrivingStyle.Rushed;
                    }
                    else
                    {
                        driving_style = DrivingStyle.Normal;
                    }
                    GTA.UI.Notify("Driving Style set to : " + driving_style);
                }

                if (vehicle_set && e.KeyCode == System.Windows.Forms.Keys.F)
                {   
                    if (Give_Tip)
                    {   
                        int ridetime_ended = Game.GameTime - ridetime_started;
                        int minimum_Money_Balance = (Game.Player.Money * 10) / 100;

                        if (minimum_Money_Balance < 100)
                        {
                            minimum_Money_Balance = 100;
                        }

                        if (Game.Player.Money > minimum_Money_Balance)
                        {   
                            UI.Notify("Ride time is : " + ridetime_ended.ToString());
                            UI.Notify("Minimum Money Balance : " + minimum_Money_Balance.ToString());
                        }
                        else
                        {
                            UI.Notify("Reached there : " + ridetime_ended.ToString());
                        }
                    }

                    if (all_peds.Count > 0)
                    {
                        foreach (var item in all_peds)
                        {
                            item.MarkAsNoLongerNeeded();
                        }
                        all_peds.Clear();
                        GTA.UI.Notify("Cleared everything");
                    }

                    vehicle_set = false;
                }
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {   
            
        }
    }//main class ends here

}//namespace ends here
