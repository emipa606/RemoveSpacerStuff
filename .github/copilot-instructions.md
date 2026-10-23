# GitHub Copilot Instructions for Remove Spacer Stuff (Continued) Mod

## Mod Overview and Purpose
**Remove Spacer Stuff (Continued)** is a RimWorld mod that rewrites the original **DarthBoers** mod in C#. The purpose of this mod is to create a more realistic setting by removing specific spacer-related items, research, factions, and pawn kinds from the game. The mod uses Harmony for patching and aims to provide more control over the game's elements.

## Key Features and Systems
- **Removed Items**: The mod disables various spacer-related items such as Marine armor, Shield Belts, Charge rifles, the Doomsday launcher, and the Triple rocket launcher. These items will not spawn on raiders, traders, or other entities.
- **Shield Belts**: These items are classified as Industrial tech and thus will not be removed.
- **Customization Options**: Players can choose to remove spacer items, research, factions, and pawn kinds through mod settings.
- **Harmony Integration**: The mod heavily relies on Harmony for patching RimWorld's core functionalities.

## Coding Patterns and Conventions
- **C# Language**: The mod is entirely written in C# to provide greater control and flexibility compared to the original XML-based mod.
- **Harmony Patching**: All modifications are made using Harmony, which allows the mod to patch RimWorld's methods without altering the original game files.
- **Settings Management**: The mod includes a settings UI that allows players to customize which spacer elements they want to remove.

## XML Integration
While the mod primarily uses C# for its implementation, it does not use any XML files. However, if you need to integrate new XML definitions or modify existing ones, here are some guidelines:

- **Def Files**: If you need to add or modify game definitions, you can do so by creating new XML files in the appropriate directories within the mod's folder.
- **Def Classes**: Ensure that any new def classes in C# are properly linked to their corresponding XML files using the `DefOf` pattern.

## Harmony Patching
Harmony is a powerful library for patching methods in RimWorld. Here are some best practices for using Harmony in your mod:

- **Patch Method Naming**: Name your patch methods clearly, indicating the original method being patched and the type of patch (e.g., `Prefix`, `Postfix`).
- **Access Modifiers**: Ensure that the original methods you are patching have appropriate access modifiers (e.g., `public`, `internal`, etc.) to allow Harmony to access them.
- **Error Handling**: Implement error handling in your patch methods to prevent crashes and ensure stability.

## Suggestions for Copilot
Here are some suggestions for GitHub Copilot to enhance the mod:

- **Code Snippets**: Provide code snippets for common tasks such as creating Harmony patches, managing mod settings, and integrating XML definitions.
- **Documentation**: Include documentation for key classes and methods to help modders understand the mod's architecture and how to extend it.
- **Examples**: Offer examples of how to add new items, research, factions, and pawn kinds to the mod.

---

By following these guidelines, you can effectively use GitHub Copilot to assist in developing and enhancing the **Remove Spacer Stuff (Continued)** mod. If you have any questions or need further assistance, please refer to the mod's documentation or the Discord channel for support.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

