#if UNITY_EDITOR || UNITY_STANDALONE
//Unity编辑器、独立应用程序（不包括Web播放器）
using Vector2F = UnityEngine.Vector2;
#elif MonoGame
//使用VS2022的MonoGame插件框架
using Vector2F = Microsoft.Xna.Framework.Vector2;
#else
using Vector2F = System.Numerics.Vector2;
#endif

namespace MetalMaxSystem
{
    /// <summary>
    /// 存储Game相关信息及方法的类
    /// </summary>
    public static class Game
    {
        #region 字段及属性

        #region EGameCatalog

        const int c_gameCatalogAbil = 0;
        const int c_gameCatalogAchievement = 1;
        const int c_gameCatalogAchievementTerm = 2;
        const int c_gameCatalogActor = 3;
        const int c_gameCatalogActorSupport = 4;
        const int c_gameCatalogAlert = 5;
        const int c_gameCatalogArmyCategory = 6;
        const int c_gameCatalogArmyUnit = 7;
        const int c_gameCatalogArmyUpgrade = 8;
        const int c_gameCatalogArtifact = 9;
        const int c_gameCatalogArtifactSlot = 10;
        const int c_gameCatalogAttachMethod = 11;
        const int c_gameCatalogBankCondition = 12;
        const int c_gameCatalogBeam = 13;
        const int c_gameCatalogBehavior = 14;
        const int c_gameCatalogBundle = 15;
        const int c_gameCatalogBoost = 16;
        const int c_gameCatalogButton = 17;
        const int c_gameCatalogCamera = 18;
        const int c_gameCatalogCampaign = 19;
        const int c_gameCatalogCharacter = 20;
        const int c_gameCatalogCliff = 21;
        const int c_gameCatalogCliffMesh = 22;
        const int c_gameCatalogColorStyle = 23;
        const int c_gameCatalogCommander = 24;
        const int c_gameCatalogConfig = 25;
        const int c_gameCatalogConsoleSkin = 26;
        const int c_gameCatalogConversation = 27;
        const int c_gameCatalogConversationState = 28;
        const int c_gameCatalogCursor = 29;
        const int c_gameCatalogDecalPack = 30;
        const int c_gameCatalogDSP = 31;
        const int c_gameCatalogEffect = 32;
        const int c_gameCatalogEmoticon = 33;
        const int c_gameCatalogEmoticonPack = 34;
        const int c_gameCatalogError = 35;
        const int c_gameCatalogFootprint = 36;
        const int c_gameCatalogFoW = 37;
        const int c_gameCatalogGame = 38;
        const int c_gameCatalogGameUI = 39;
        const int c_gameCatalogHerd = 40;
        const int c_gameCatalogHerdNode = 41;
        const int c_gameCatalogHero = 42;
        const int c_gameCatalogHeroAbil = 43;
        const int c_gameCatalogHeroStat = 44;
        const int c_gameCatalogItem = 45;
        const int c_gameCatalogItemClass = 46;
        const int c_gameCatalogItemContainer = 47;
        const int c_gameCatalogKinetic = 48;
        const int c_gameCatalogLensFlareSet = 49;
        const int c_gameCatalogLight = 50;
        const int c_gameCatalogLocation = 51;
        const int c_gameCatalogLoot = 52;
        const int c_gameCatalogMap = 53;
        const int c_gameCatalogModel = 54;
        const int c_gameCatalogMount = 55;
        const int c_gameCatalogMover = 56;
        const int c_gameCatalogObjective = 57;
        const int c_gameCatalogPhysicsMaterial = 58;
        const int c_gameCatalogPing = 59;
        const int c_gameCatalogPortraitPack = 60;
        const int c_gameCatalogPreload = 61;
        const int c_gameCatalogPremiumMap = 62;
        const int c_gameCatalogRace = 63;
        const int c_gameCatalogRaceBannerPack = 64;
        const int c_gameCatalogRequirement = 65;
        const int c_gameCatalogRequirementNode = 66;
        const int c_gameCatalogReverb = 67;
        const int c_gameCatalogReward = 68;
        const int c_gameCatalogScoreResult = 69;
        const int c_gameCatalogScoreValue = 70;
        const int c_gameCatalogShape = 71;
        const int c_gameCatalogSkin = 72;
        const int c_gameCatalogSkinPack = 73;
        const int c_gameCatalogSound = 74;
        const int c_gameCatalogSoundExclusivity = 75;
        const int c_gameCatalogSoundMixSnapshot = 76;
        const int c_gameCatalogSoundtrack = 77;
        const int c_gameCatalogSpray = 78;
        const int c_gameCatalogSprayPack = 79;
        const int c_gameCatalogTacCooldown = 80;
        const int c_gameCatalogTactical = 81;
        const int c_gameCatalogTalent = 82;
        const int c_gameCatalogTalentProfile = 83;
        const int c_gameCatalogTargetFind = 84;
        const int c_gameCatalogTargetSort = 85;
        const int c_gameCatalogTerrain = 86;
        const int c_gameCatalogTerrainObject = 87;
        const int c_gameCatalogTerrainTex = 88;
        const int c_gameCatalogTexture = 89;
        const int c_gameCatalogTextureSheet = 90;
        const int c_gameCatalogTile = 91;
        const int c_gameCatalogTrophy = 92;
        const int c_gameCatalogTurret = 93;
        const int c_gameCatalogUnit = 94;
        const int c_gameCatalogUpgrade = 95;
        const int c_gameCatalogUser = 96;
        const int c_gameCatalogValidator = 97;
        const int c_gameCatalogVoiceOver = 98;
        const int c_gameCatalogVoicePack = 99;
        const int c_gameCatalogWarChest = 100;
        const int c_gameCatalogWarChestSeason = 101;
        const int c_gameCatalogWater = 102;
        const int c_gameCatalogWeapon = 103;
        const int c_gameCatalogStimPack = 104;
        const int c_gameCatalogAccumulator = 105;
        const int c_gameCatalogPlayerResponse = 106;
        const int c_gameCatalogDataCollection = 107;
        const int c_gameCatalogDataCollectionPattern = 108;
        const string c_gameCatalogAbilName = "Abil";
        const string c_gameCatalogAchievementName = "Achievement";
        const string c_gameCatalogAchievementTermName = "AchievementTerm";
        const string c_gameCatalogActorName = "Actor";
        const string c_gameCatalogActorSupportName = "ActorSupport";
        const string c_gameCatalogAlertName = "Alert";
        const string c_gameCatalogArmyCategoryName = "ArmyCategory";
        const string c_gameCatalogArmyUnitName = "ArmyUnit";
        const string c_gameCatalogArmyUpgradeName = "ArmyUpgrade";
        const string c_gameCatalogArtifactName = "Artifact";
        const string c_gameCatalogArtifactSlotName = "ArtifactSlot";
        const string c_gameCatalogAttachMethodName = "AttachMethod";
        const string c_gameCatalogBankConditionName = "BankCondition";
        const string c_gameCatalogBeamName = "Beam";
        const string c_gameCatalogBehaviorName = "Behavior";
        const string c_gameCatalogBundleName = "Bundle";
        const string c_gameCatalogBoostName = "Boost";
        const string c_gameCatalogButtonName = "Button";
        const string c_gameCatalogCameraName = "Camera";
        const string c_gameCatalogCampaignName = "Campaign";
        const string c_gameCatalogCharacterName = "Character";
        const string c_gameCatalogCliffName = "Cliff";
        const string c_gameCatalogCliffMeshName = "CliffMesh";
        const string c_gameCatalogColorStyleName = "ColorStyle";
        const string c_gameCatalogCommanderName = "Commander";
        const string c_gameCatalogConfigName = "Config";
        const string c_gameCatalogConsoleSkinName = "ConsoleSkin";
        const string c_gameCatalogConversationName = "Conversation";
        const string c_gameCatalogConversationStateName = "ConversationState";
        const string c_gameCatalogCursorName = "Cursor";
        const string c_gameCatalogDecalPackName = "DecalPack";
        const string c_gameCatalogDSPName = "DSP";
        const string c_gameCatalogEffectName = "Effect";
        const string c_gameCatalogEmoticonName = "Emoticon";
        const string c_gameCatalogEmoticonPackName = "EmoticonPack";
        const string c_gameCatalogErrorName = "Error";
        const string c_gameCatalogFootprintName = "Footprint";
        const string c_gameCatalogFoWName = "FoW";
        const string c_gameCatalogGameName = "Game";
        const string c_gameCatalogGameUIName = "GameUI";
        const string c_gameCatalogHerdName = "Herd";
        const string c_gameCatalogHerdNodeName = "HerdNode";
        const string c_gameCatalogHeroName = "Hero";
        const string c_gameCatalogHeroAbilName = "HeroAbil";
        const string c_gameCatalogHeroStatName = "HeroStat";
        const string c_gameCatalogItemName = "Item";
        const string c_gameCatalogItemClassName = "ItemClass";
        const string c_gameCatalogItemContainerName = "ItemContainer";
        const string c_gameCatalogKineticName = "Kinetic";
        const string c_gameCatalogLensFlareSetName = "LensFlareSet";
        const string c_gameCatalogLightName = "Light";
        const string c_gameCatalogLocationName = "Location";
        const string c_gameCatalogLootName = "Loot";
        const string c_gameCatalogMapName = "Map";
        const string c_gameCatalogModelName = "Model";
        const string c_gameCatalogMountName = "Mount";
        const string c_gameCatalogMoverName = "Mover";
        const string c_gameCatalogObjectiveName = "Objective";
        const string c_gameCatalogPhysicsMaterialName = "PhysicsMaterial";
        const string c_gameCatalogPingName = "Ping";
        const string c_gameCatalogPortraitPackName = "PortraitPack";
        const string c_gameCatalogPreloadName = "Preload";
        const string c_gameCatalogPremiumMapName = "PremiumMap";
        const string c_gameCatalogRaceName = "Race";
        const string c_gameCatalogRaceBannerPackName = "RaceBannerPack";
        const string c_gameCatalogRequirementName = "Requirement";
        const string c_gameCatalogRequirementNodeName = "RequirementNode";
        const string c_gameCatalogReverbName = "Reverb";
        const string c_gameCatalogRewardName = "Reward";
        const string c_gameCatalogScoreResultName = "ScoreResult";
        const string c_gameCatalogScoreValueName = "ScoreValue";
        const string c_gameCatalogShapeName = "Shape";
        const string c_gameCatalogSkinName = "Skin";
        const string c_gameCatalogSkinPackName = "SkinPack";
        const string c_gameCatalogSoundName = "Sound";
        const string c_gameCatalogSoundExclusivityName = "SoundExclusivity";
        const string c_gameCatalogSoundMixSnapshotName = "SoundMixSnapshot";
        const string c_gameCatalogSoundtrackName = "Soundtrack";
        const string c_gameCatalogSprayName = "Spray";
        const string c_gameCatalogSprayPackName = "SprayPack";
        const string c_gameCatalogTacCooldownName = "TacCooldown";
        const string c_gameCatalogTacticalName = "Tactical";
        const string c_gameCatalogTalentName = "Talent";
        const string c_gameCatalogTalentProfileName = "TalentProfile";
        const string c_gameCatalogTargetFindName = "TargetFind";
        const string c_gameCatalogTargetSortName = "TargetSort";
        const string c_gameCatalogTerrainName = "Terrain";
        const string c_gameCatalogTerrainObjectName = "TerrainObject";
        const string c_gameCatalogTerrainTexName = "TerrainTex";
        const string c_gameCatalogTextureName = "Texture";
        const string c_gameCatalogTextureSheetName = "TextureSheet";
        const string c_gameCatalogTileName = "Tile";
        const string c_gameCatalogTrophyName = "Trophy";
        const string c_gameCatalogTurretName = "Turret";
        const string c_gameCatalogUnitName = "Unit";
        const string c_gameCatalogUpgradeName = "Upgrade";
        const string c_gameCatalogUserName = "User";
        const string c_gameCatalogValidatorName = "Validator";
        const string c_gameCatalogVoiceOverName = "VoiceOver";
        const string c_gameCatalogVoicePackName = "VoicePack";
        const string c_gameCatalogWarChestName = "WarChest";
        const string c_gameCatalogWarChestSeasonName = "WarChestSeason";
        const string c_gameCatalogWaterName = "Water";
        const string c_gameCatalogWeaponName = "Weapon";
        const string c_gameCatalogStimPackName = "StimPack";
        const string c_gameCatalogAccumulatorName = "Accumulator";
        const string c_gameCatalogPlayerResponseName = "PlayerResponse";
        const string c_gameCatalogDataCollectionName = "DataCollection";
        const string c_gameCatalogDataCollectionPatternName = "DataCollectionPattern";

        #endregion

        #region 地图相关

        private static float[,] _terrainHeight = new float[2560 + 1, 2560 + 1];

        /// <summary>
        /// 地图首个纹理图层顶面高度，默认值=8（m），亦称地面高度或地图高度
        /// </summary>
        public static float MapHeight { get; set; }

        /// <summary>
        /// 地面上附加的悬崖、地形物件的高度，二维坐标数组元素[2560+1,2560+1]（设计精度0.1m，按256m计）
        /// </summary>
        public static float[,] TerrainHeight
        {
            get
            {
                return _terrainHeight;
            }

            set
            {
                _terrainHeight = value;
            }
        }

        /// <summary>
        /// 土、矿、水、气等空间内每个点的属性类型和数量（密度），数组元素[2560+1,2560+1,2560+1]，设计精度0.1m，小数点左侧表示土的类型，右侧为数值（密度）
        /// </summary>
        public static float[,,] TerrainType { get; set; }

        #endregion

        #region 其他

        /// <summary>
        /// 载具类型上限
        /// </summary>
        public const int c_vehicleTypeMax = 200;

        /// <summary>
        /// 任意玩家编号（玩家编号从0-15共16个，16是上帝由系统执行，某些函数中也作"任意玩家"参数）
        /// </summary>
        public const int c_playerAny = 16;

        /// <summary>
        /// 玩家编号上限（限制最大玩家数）
        /// </summary>
        public const int c_maxPlayers = 16;

        /// <summary>
        /// 本地用户玩家的编号
        /// </summary>
        public static int UID { get; set; }

        /// <summary>
        /// 最近一次新建的单位句柄
        /// </summary>
        public static int CurrentUnitHandle { get; set; }

        /// <summary>
        /// 最近一次新建的单位
        /// </summary>
        public static Unit UnitLastCreated { get; set; }

        /// <summary>
        /// 初始化阶段
        /// </summary>
        public static int Initialization { get; set; }

        #endregion

        #endregion

        #region 函数

        /// <summary>
        /// 设置指定目录数据类型的字段值
        /// </summary>
        /// <param name="catalog">目录</param>
        /// <param name="entry">数据类型</param>
        /// <param name="fieldPath">字段路径</param>
        /// <param name="player">玩家</param>
        /// <param name="value">值</param>
        public static void CatalogFieldValueSet(int catalog, string entry, string fieldPath, int player, string value)
        {
            MMCore.DataTableStringSave0(true, catalog.ToString() + "_" + entry + "_" + fieldPath + "_" + player, value);
        }

        /// <summary>
        /// 获取指定目录数据类型的字段值
        /// </summary>
        /// <param name="catalog">目录</param>
        /// <param name="entry">数据类型</param>
        /// <param name="fieldPath">字段路径</param>
        /// <param name="player">玩家</param>
        /// <returns></returns>
        public static string CatalogFieldValueGet(int catalog, string entry, string fieldPath, int player)
        {
            return MMCore.DataTableStringLoad0(true, catalog.ToString() + "_" + entry + "_" + fieldPath + "_" + player);
        }

        /// <summary>
        /// 创建单位
        /// </summary>
        /// <param name="player"></param>
        /// <param name="unitType">字符串类型名称</param>
        /// <param name="vector"></param>
        /// <param name="unitCreateTag"></param>
        /// <returns>返回根据unitType创建的单位实例</returns>
        public static Unit UnitCreate(string unitType, UnitCreateTag unitCreateTag, int player, Vector2F vector)
        {
            Unit unit = new Unit(unitType);
            unit.Owner = player;
            unit.Vector2F = vector;
            if (unit.Hp <= 0.0) { unit.Hp = 1.0; }
            return unit;
        }

        /// <summary>
        /// 创建AI指令
        /// </summary>
        /// <param name="player"></param>
        /// <param name="abilLink"></param>
        /// <param name="abilIndex"></param>
        /// <returns></returns>
        public static Order AICreateOrder(int player, string abilLink, int abilIndex)
        {
            Abilcmd cmd = AbilityCommand(abilLink, abilIndex);
            if (cmd == null)
            {
                return null;
            }
            Order ord = new Order(cmd);
            ord.Player = player;
            return ord;
        }

        public static Abilcmd AbilityCommand(string abilLink, int abilIndex)
        {
            return new Abilcmd(abilLink, abilIndex);
        }

        public static Order OrderTargetingPoint(Abilcmd inAbilCmd, Vector2F inPoint)
        {
            Order ord = new Order(inAbilCmd);
            ord.TargetType = 2;
            ord.TargetVector = inPoint;
            return ord;
        }

        #endregion
    }
}
