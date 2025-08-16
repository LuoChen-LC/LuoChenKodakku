using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using ECommons;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;

namespace LuoChen_Kodakku._07_DawnTrail
{
    [ScriptType(name: "新月岛", territorys: [1252], guid: "5ec11a8a-ca58-49c3-8f5d-be6a2a1782f6", version: "0.0.0.1",
        author: "LuoChen", note: noteStr)]
    public class 新月岛CE
    {
        const string noteStr =
            """
            画着玩玩
            """;
        [UserSetting("DebugMode")] public bool debugMode { get; set; } = true;
        public void Init(ScriptAccessory accessory)
        {
            _spinningSiegeCount = [];
        }

        public void DebugMsg(string str, ScriptAccessory accessory)
        {
            if (!debugMode)
                return;
            accessory.Method.SendChat($"/e [DEBUG] {str}");
        }

        #region 石质骑士团

        private List<Vector3> _spinningSiegeCount = [];

        [ScriptMethod(name: "回旋炮_指路（石质骑士团）", eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^41823"])]
        public void 回旋炮_起始指路(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                _spinningSiegeCount.Add(@event.SourcePosition);
                if (_spinningSiegeCount.Count != 2)
                    return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "回旋炮_起始指路";
                dp.Scale = new Vector2(2);
                dp.Owner = accessory.Data.Me;
                Vector3 pos1 = _spinningSiegeCount[0];
                Vector3 pos2 = _spinningSiegeCount[1];
                Vector3 tPos = Vector3.Zero;

                if (pos1.Z.Equals(pos2.Z))
                {
                    tPos = pos1.Z < -280
                        ? new Vector3(675, 96, -280) // 西
                        : new Vector3(685, 96, -280); // 东
                }
                else if (pos1.X.Equals(pos2.X))
                {
                    tPos = pos1.X > 680
                        ? new Vector3(680, 96, -285) // 北
                        : new Vector3(680, 96, -275); // 南
                }

                dp.TargetPosition = tPos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                _spinningSiegeCount.Clear();
            }
        }

        //地火
        [ScriptMethod(name: "重拳波（石质骑士团）", eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^41827"])]
        public void 重拳波(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "重拳波";
            dp.Scale = new Vector2(9);
            dp.Position = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "重拳崩（石质骑士团）", eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^41828"])]
        public void 重拳崩(Event @event, ScriptAccessory accessory)
        {
            Vector3 effectPosition = @event.EffectPosition;
            float rotation = @event.SourceRotation;
            Vector3 currentPosition = effectPosition;
            Vector3 offset = new Vector3(MathF.Sin(rotation), 0, MathF.Cos(rotation)) * 7f;
            for (int i = 0; i < 6; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "重拳崩";
                dp.Scale = new Vector2(6);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = currentPosition;

                dp.Delay = (i == 0 ? 0 : (i - 1) * 1000 + 10000);
                dp.DestoryAt = (i == 0 ? 10000 : 1000);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                currentPosition += offset;
            }
        }

        #endregion
    }
}