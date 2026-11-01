# ECHO SPHERE economy sensitivity model

Run from repository root with .NET8:

```
dotnet run --project tools/EchoSphere.BalanceSim -c Release -- 64 UnityProject/Library/economy-v1.json 0
dotnet run --project tools/EchoSphere.BalanceSim -c Release -- 64 UnityProject/Library/economy-v2.json 2
```

Arguments: seeds per region/profile/policy; output JSON path; pricing version.
Each command runs9,216 scenarios:6regions ×4throughput profiles ×3purchase
policies ×64seeds ×2exploration states. CSV rows are written beside the summary.
Version0 reproduces the preceding100/180/300/500price table; version2 uses
250/450/750/1250. Existing saved stocks and old economy states retain version0.

The model directly executes production Core wave sizes/intervals, archetype and
elite XP/drop rules, boss XP, merchant inventory/discount/transactions, artifact
reward draws, exploration reservation and snapshot codec. Every visit checks
wallet accounting, duplicate ownership, double-purchase rejection and restoration.
CI runs288smoke scenarios; the checked-in report records the larger comparison.

This is NOT a combat simulator. Fixed assumed enemy lifetimes .5/3/8/18seconds
stand in for player damage/AI; collection chances are1/.95/.8/.55. Normal difficulty,
neutral pocket XP, 5second pickup delay, boss rewards at regional schedule+15s
and final at1750s; two exploration discoveries at600/1200s. Discovery positions,
POI travel time, survival, character/sphere/ability/artifact builds and their effect
on later kill speed are not simulated. Rune frequency counts potential drops;
collection/expiry/tactical usefulness require actual play. Special cells are
sampled separately and are not granted in ordinary stabilization.

The20–23artifact range is a reference, never a hard cap. A low-throughput run
can earn fewer; fast cheap-item strategies can exceed it. Do not treat these
assumed profiles as telemetry from Xiaomi11T or as proof of full-game balance.
