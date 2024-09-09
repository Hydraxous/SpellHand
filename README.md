# SpellHand
SpellHand is a mod that implements a visual overhaul of spellcasting. It adds a left hand which adorns your equipped magic rings.
The spell hand is animated and has many different animations depending on which spell is being cast.

<img src="https://raw.githubusercontent.com/Hydraxous/SpellHand/master/DemoAssets/Preview.PNG" alt="Hand Preview" width="400"/>

[Watch a Demo Video Here](https://youtu.be/xCf2MlaFNWk)

## Important Info
- The mod does not make any changes to spell functionality, it only plays visual animations to match the spell usage of the player.
- The mod by default disables the charging reticle for spells, if you want to stop this behavior you can disable it in the Bepinex/config for the mod under DisableCrosshairOverlay
- You can customize the target animation for each spell by modifying AnimationMapping.txt file in the SpellHand folder.
- The hand's rings by default are set to appear in the same left to right order as the spells HUD and will maintain this order when using the vanilla flipped hand option.
- When you are unable to cast a spell due to lack of mana or blood, the glint from that spell ring will fade out.
- When you are charging a spell a small animation will play on the glint of the ring to indicate charging status.
  
## Configuration
- DisableCrosshairOverlay (default:True) -> Disables the center screen magic reticle when charging spells.
- FlipRingPositions (default:False) -> Flips the left-right positions of the rings on the hand.

# How to install
1.  Install BepInEx 5.4.21 into your LUNACID files
2. Go to releases and download the [latest release](https://github.com/Hydraxous/SpellHand/releases/latest)
3. Extract the mod files into the Lunacid/BepInEx/ folder
4. Start the game
5. Enjoy
