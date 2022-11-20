using CupheadTAS.Components;
using TAS;
using TAS.Core.Input.Commands;

namespace CupheadTAS.Commands;

public class LoadCommand : PluginComponent {
    [TasCommand("Load", LegalInMainGame = false)]
    private static void LoadLevel(string[] args) {
        if (args.Length == 0) {
            return;
        }
        
        string sceneName = args[0];

        if (!PlayerData.inGame && sceneName != "scene_title" && sceneName != "scene_slot_select") {
            Toast.Show("Load Command Failed\nPlease select an save first");
            Manager.DisableRunLater();
            return;
        }

        if (sceneName is "scene_title" or "scene_slot_select") {
            PlayerManager.ResetPlayers();
        }

        SceneLoader.currentlyLoading = false;
        SceneLoader.instance.StopAllCoroutines();

        SceneLoader.Transition transitionStart = SceneLoader.Transition.Iris;
        if (GetLevels(sceneName) is { } levels) {
            SceneLoader.LoadLevel(levels, transitionStart, SceneLoader.Icon.None);
        } else {
            SceneLoader.LoadScene(sceneName, transitionStart, SceneLoader.Transition.Iris, SceneLoader.Icon.None);
        }
    }

    public static Levels? GetLevels(string sceneName) {
        return sceneName switch {
            "scene_level_test" => Levels.Test,
            "scene_level_flying_test" => Levels.FlyingTest,
            "scene_level_tutorial" => Levels.Tutorial,
            "scene_level_pirate" => Levels.Pirate,
            "scene_level_bat" => Levels.Bat,
            "scene_level_train" => Levels.Train,
            "scene_level_veggies" => Levels.Veggies,
            "scene_level_frogs" => Levels.Frogs,
            "scene_level_bee" => Levels.Bee,
            "scene_level_mouse" => Levels.Mouse,
            "scene_level_dragon" => Levels.Dragon,
            "scene_level_flower" => Levels.Flower,
            "scene_level_slime" => Levels.Slime,
            "scene_level_baroness" => Levels.Baroness,
            "scene_level_airship_jelly" => Levels.AirshipJelly,
            "scene_level_airship_stork" => Levels.AirshipStork,
            "scene_level_airship_crab" => Levels.AirshipCrab,
            "scene_level_flying_bird" => Levels.FlyingBird,
            "scene_level_flying_mermaid" => Levels.FlyingMermaid,
            "scene_level_flying_blimp" => Levels.FlyingBlimp,
            "scene_level_robot" => Levels.Robot,
            "scene_level_clown" => Levels.Clown,
            "scene_level_sally_stage_play" => Levels.SallyStagePlay,
            "scene_level_dice_palace_domino" => Levels.DicePalaceDomino,
            "scene_level_dice_palace_card" => Levels.DicePalaceCard,
            "scene_level_dice_palace_chips" => Levels.DicePalaceChips,
            "scene_level_dice_palace_cigar" => Levels.DicePalaceCigar,
            "scene_level_dice_palace_test" => Levels.DicePalaceTest,
            "scene_level_dice_palace_booze" => Levels.DicePalaceBooze,
            "scene_level_dice_palace_roulette" => Levels.DicePalaceRoulette,
            "scene_level_dice_palace_pachinko" => Levels.DicePalacePachinko,
            "scene_level_dice_palace_rabbit" => Levels.DicePalaceRabbit,
            "scene_level_airship_clam" => Levels.AirshipClam,
            "scene_level_flying_genie" => Levels.FlyingGenie,
            "scene_level_dice_palace_light" => Levels.DicePalaceLight,
            "scene_level_dice_palace_flying_horse" => Levels.DicePalaceFlyingHorse,
            "scene_level_dice_palace_flying_memory" => Levels.DicePalaceFlyingMemory,
            "scene_level_dice_palace_main" => Levels.DicePalaceMain,
            "scene_level_dice_palace_eight_ball" => Levels.DicePalaceEightBall,
            "scene_level_devil" => Levels.Devil,
            "scene_level_retro_arcade" => Levels.RetroArcade,
            "scene_level_mausoleum" => Levels.Mausoleum,
            "scene_level_house_elder_kettle" => Levels.House,
            "scene_level_dice_gate" => Levels.DiceGate,
            "scene_level_shmup_tutorial" => Levels.ShmupTutorial,
#if v134
            "scene_level_airplane" => Levels.Airplane,
            "scene_level_rum_runners" => Levels.RumRunners,
            "scene_level_old_man" => Levels.OldMan,
            "scene_level_chess_bishop" => Levels.ChessBishop,
            "scene_level_snow_cult" => Levels.SnowCult,
            "scene_level_flying_cowboy" => Levels.FlyingCowboy,
            "scene_level_tower_of_power" => Levels.TowerOfPower,
            "scene_level_chess_bolda" => Levels.ChessBOldA,
            "scene_level_chess_knight" => Levels.ChessKnight,
            "scene_level_chess_rook" => Levels.ChessRook,
            "scene_level_chess_queen" => Levels.ChessQueen,
            "scene_level_chess_pawn" => Levels.ChessPawn,
            "scene_level_chess_king" => Levels.ChessKing,
            "scene_level_kitchen" => Levels.Kitchen,
            "scene_level_chess_boldb" => Levels.ChessBOldB,
            "scene_level_saltbaker" => Levels.Saltbaker,
            "scene_level_chalice_tutorial" => Levels.ChaliceTutorial,
            "scene_level_graveyard" => Levels.Graveyard,
            "scene_level_chess_castle" => Levels.ChessCastle,
#endif
            "scene_level_platforming_1_1F" => Levels.Platforming_Level_1_1,
            "scene_level_platforming_1_2F" => Levels.Platforming_Level_1_2,
            "scene_level_platforming_3_1F" => Levels.Platforming_Level_3_1,
            "scene_level_platforming_3_2F" => Levels.Platforming_Level_3_2,
            "scene_level_platforming_2_2F" => Levels.Platforming_Level_2_2,
            "scene_level_platforming_2_1F" => Levels.Platforming_Level_2_1,
            _ => null
        };
    }
}