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

        public static bool Mod_Active = false;

        public Ped Player_Ped = GTA.Game.Player.Character;

        private List<Ped> all_peds = new List<Ped>();

        private bool vehicle_set = false;

        private int speed = 10;

        private DrivingStyle driving_style = DrivingStyle.Normal;

        private Vector3 Destionation;


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
                    Destionation = wpVec.Around(10f);
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
                    //UI.ShowSubtitle("Mod Activated : " + Mod_Name);
                    if (Player_Ped != null)
                    {
                        if (Player_Ped.IsOnFoot)
                        {   
                            var nearest_vehicle = World.GetNearbyVehicles(Player_Ped.Position, 3f);

                            for (int i = 0; i < nearest_vehicle.Length; i++)
                            {
                                if (nearest_vehicle[i] != null)
                                {
                                    if (nearest_vehicle[i].GetPedOnSeat(VehicleSeat.Driver) != null && nearest_vehicle[i].IsSeatFree(VehicleSeat.Any))
                                    {
                                        all_peds = nearest_vehicle[i].Passengers.ToList();                                        
                                        all_peds.Add(nearest_vehicle[i].GetPedOnSeat(VehicleSeat.Driver));
                                        
                                        foreach (var item in all_peds)
                                        {
                                            Function.Call(GTA.Native.Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, 2, Player_Ped.RelationshipGroup, item.RelationshipGroup);
                                            Function.Call(GTA.Native.Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, 2, item.RelationshipGroup, Player_Ped.RelationshipGroup);
                                            //World.SetRelationshipBetweenGroups(Relationship.Like, item.RelationshipGroup, Player_Ped.RelationshipGroup);
                                            Function.Call(GTA.Native.Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, item, 1);
                                            Function.Call(GTA.Native.Hash.SET_PED_COMBAT_ATTRIBUTES, item, 20, true);
                                        }
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
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

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
                    if (all_peds.Count > 0)
                    {   
                        foreach (var item in all_peds)
                        {
                            item.MarkAsNoLongerNeeded();
                        }
                        all_peds.Clear();
                        GTA.UI.Notify("Cleared everything");
                    }
                }
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {   
            
        }
    }//main class ends here

}//namespace ends here
