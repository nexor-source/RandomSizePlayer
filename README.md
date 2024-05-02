# Random Size Player

It is highly recommended to use with [**LethalConfig** **by AinaVT**](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/), as this mod allows in-game modification of the parameters in parameters in config, which **allows the player to reopen the room again to implement new character sizes without having to quit the game**, and this **allows the player to adjust the parameters for placing furniture sizes in real time** within the game, which is VERY VERY  convenient!

强烈建议搭配 [**LethalConfig** **by AinaVT**](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/)使用，因为该mod可以在游戏内修改config中的参数，这使得玩家**不用退出游戏就可以再重新开启房间实施新的人物大小**，并且这可以使得玩家**在游戏内实时调整放置家具大小的参数**，十分方便！

## Features  (英文版)

### **[1] Player size modification (server-side only)**

As a host, when a new player join your room it will be applied with a customized size. You can:

- **give a fixed value for the aspect ratio that will be applied whenever a player joins the room.**

  Note that a player that is too large (>?????) or too short (<0.18) may cause the player to get stuck

- **Use a random size that is applied whenever a player joins.**

  In order to ensure that players can play properly, the random size is limited to a range of (0,1.6), which makes it possible for players to crouch sideways to enter a door even at the maximum size without causing them to be unable to enter the door, or exit the ship, and not get stuck at the minimum size.
  There are also two additional parameters in the config file:

  **`lock portion`**: since the width, height, and thickness are randomized separately when random sizes are used, set this to `true` if you want to lock the generated proportions.

  **`sharpness of random player size's curve`**: control the distribution of random size, by default, the distribution curve of random size is like the red curve in the figure (x-axis is the value of the random variable, y-axis is the proportion):

  > When generating a random size, generate 3 (-1,1) random numbers for width, height and thickness respectively, and then directly bring in the corresponding curve to calculate the corresponding size change ratio. The sharpness (1) actually multiplies the coefficient of `-4i` in the formula.

  ![curve](https://imgur.com/NEj3BfS.png)

  The lower the sharpness the more the curve tends to be the straight blue line, making the randomized values closer to the median and less likely to have extreme values close to 0.2 or 1.6, while the higher the sharpness the more the curve tends to be the black curve, making the probability of the median value much lower and the probability of the extreme value close to 0.2 or 1.6 much higher.

  In short, you generally don't need to manually adjust the sharpness of a curve unless you want a more extreme distribution or a more uniform distribution of the player's random size.

Notes:

1. as the thickness of the player decreases, the player's visor becomes larger thus obscuring more of the field of view.
2. too tall players need to crouch to enter the door, some tall and wide/thick players may need to crouch + turn sideways to enter the door
3. too small players can easily be blocked from view after picking up the waste, and small players can easily fall into the gap and die.
4. too short players will not be scanned as targets by turrets.
5. players' movement and jumping speed are not affected by their size.

### [2] Furniture size modification (server-side only and still under test)

- **Modifying the size of furniture**

  As a room owner, whenever you place a piece of furniture, the furniture applies the scaling given from the config file. When a client joins your room, it will synchronize all furniture sizes with you, and it will **not be able to synchronize subsequent modifications made by the host after joining the room**.

- **Save preset file for each furniture size** 

  Since furniture sizes revert to their default sizes every time you open a room, whenever you as the host place a piece of furniture, the size parameters you apply to it are recorded in a furniture size preset file called `save size-preset-file's name`. You can adjust the value of `load preset size file` to determine whether or not to load the furniture size preset file called `load size-preset-file's name` when you open the room. All of these parameters can be adjusted in the config file. As for these preset files, they are saved to the path `C:\xxxxx\xxxxx\AppData\LocalLow\ZeekerssRBLX\Lethal Company`.

Notes:

1. oversized furniture can result in it being placed outside the ship and its seemingly **unable to be placed again**.
2. the actual size of the ghost furniture when in placement mode seems to be the same as the original default size, and as such can easily lead to the appearance of mold-through.

## Features（中文版）

### 【1】玩家大小的修改（仅服务端有效）

作为房主，你房间加入新的玩家时其会被应用以一个自定义的大小。你可以：

- **给定宽高厚变化比例的固定值，每当玩家加入时则会应用这个固定值**

  需要注意的是，过大(>???)或者过矮(<0.18)可能会造成玩家被卡住的情况

- **使用随机大小，每当玩家加入时则会应用这个随机大小。**

  为了保证玩家可以正常的游戏，随机大小的范围限定在了(0,1.6)的范围内，这使得玩家在最大大小下也可以蹲下侧身来进门，而不会导致玩家无法进门，或者是无法出飞船，且在最小大小下不会被卡住。
  在config文件中还额外有两个参数：

  `lock portion`：由于随机大小时的宽高厚会分别随机，因此如果你想锁定生成的比例，则可以将此项设置为`true`。

  `sharpness of random player size's curve`：控制随机大小的分布，默认情况下，随机大小的分布曲线如图中的红色曲线（x轴是随机变量值，y轴是变化比例）：

  > 生成一个随机大小时，分别为宽高厚生成3个(-1,1)的随机数，然后直接带入对应的曲线算出对应的大小变化比例。这里的sharpness影响的就是公式中`i`的系数。

  ![curve](https://imgur.com/NEj3BfS.png)

  sharpness越低曲线就越趋于蓝色直线，使得随机得到的值更加接近中间值，更少出现接近0.2/1.6的极端值；而sharpness越高曲线就越趋于黑色曲线，使得中间值出现的概率大大降低，而接近0.2/1.6的极端值出现的概率则会大大升高。

  总之，你一般不需要手动调整曲线的sharpness值，除非你想要更加极端的分布或者更加均匀的分布。

注意：

1. 当玩家的厚度降低时，玩家的visor会变得更大从而遮挡更多的视野。
2. 过高的玩家进门需要蹲下，有的又高又宽/厚的玩家可能需要蹲下+侧身才能进门
3. 过小的玩家在抱起废料后容易被遮挡视线，并且小玩家容易掉入缝隙中摔死。
4. 过矮的玩家不会被机枪扫描为目标。
5. 玩家的移动和跳跃速度并不会受到体型的影响

### 【2】家具大小的修改（仅服务端有效，且在测试中）

- **修改家具的大小**

  作为房主，每当你放置一个家具后，家具都会应用来自config文件中给定的缩放比例。某客户端加入你的房间时，其会和你同步所有的家具大小，且其在加入房间后没法同步房主后续做出的大小修改操作。

- **保存各家具大小的预设文件**

  由于每次开启房间时家具的大小都会恢复为默认大小，因此每当你作为房主放下一个新的家具后，你应用于其上的大小参数会被记录到一个名为`save size-preset-file's name`的家具大小预设文件中。你可以调整`load preset size file`的值来决定是否在开启房间时加载名为`load size-preset-file's name`的家具大小预设文件。这些参数都可以在config文件中进行调整。至于这些预设文件，它们会被保存到`C:\xxxx\xxxx\AppData\LocalLow\ZeekerssRBLX\Lethal Company`的路径下。

注意：

1. 过大的家具会导致其被放置在飞船外面，并且其似乎**无法再被放置**。
2. 家具在放置模式时的虚影的实际大小似乎还是原本的默认大小，因此会很容易导致穿模的出现。

> special thanks to @怂熊(知识学爆版）
>
> 特别感谢  @怂熊(知识学爆版） 的指导。

