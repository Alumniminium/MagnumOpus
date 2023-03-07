# Co2 Research Notes


### Equipment

* You must send the MsgItem packet w/ SetEquipPosition (5) *BEFORE* removing the item from the inventory, otherwise the client will not show the equipment update. Eg. if you try to wear a dress, you will still be naked.

### CQ_GENERATOR

* born_x is start_x and born_x + born_cx = end_x
* born_y is start_y and born_y + born_cy = end_x
* max_per_gen is the amount of mobs to spawn each interval
* rest_secs is the amount of seconds between each interval
* max_npc is the maximum amount of living monsters -> skip interval.
* npctype is the cq_monstertype Id of the monster to spawn

#### CQ_GENERATOR: The Unknowns

* Titan, Ganoderma, are not working right. They depend on a cq_action or cq_task as far as I know.
* Guard entries used to have high max_npc, like 100, or really big spawn areas. I reduced max_npc to 1. 
* Pheasant spawns for example, have rest_secs = 1 which means monsters respawn really quickly.

Maybe that's action related? How does TQ do it? Do generators run their own timers or are all generators controlled by cq_actions?

### CQ_MONSTERTYPE

* action column has cq_action id in it. Primarily used for special item drops.

#### CQ_MONSTERTYPE: The Unknowns

* AI TYpe, STC Type, ... loads of unknowns.
