# MagnumOpus TODO List

## Testing Tasks

- [ ] **Test Bow + Arrow Equip change to Non Bow weapon change**
  - Verify that when a player has a bow equipped with arrows in the left weapon slot
  - And they equip a non-bow weapon (sword, blade, etc.) to the right weapon slot
  - The arrows are automatically unequipped and moved back to inventory
  - Network packets are sent correctly for both weapon equip and arrow unequip
  - Inventory space validation works (if inventory full, prevent weapon change)
  - Logging messages appear when auto-unequipping arrows

## Development Tasks

- [ ] Add more system cleanup following established pattern
- [ ] Implement additional equipment validation rules
- [ ] Add unit tests for equipment system

## Performance Tasks

- [ ] Profile equipment system performance under load
- [ ] Optimize inventory helper methods
- [ ] Review spatial hash update frequency