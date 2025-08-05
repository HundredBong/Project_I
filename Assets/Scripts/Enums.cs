public enum EnemyType { None = -1, Melee = 0, Range = 1, Boss = 2 }

public enum EnemyId { Skeleton_Knight, Skeleton_Archer, Skeleton_Warrior, Skeleton_Shield }

public enum StatUpgradeType { Attack, Health, AttackSpeed, MoveSpeed, }

public enum GoldUpgradeType { Attack, Health, CriticalChance, CriticalDamage }

public enum PlayerProgressType { Level, CurrentExp, MaxExp, StatPoint, Gold, Diamond, EnhanceStone, SkillGem, EnhanceDungeonTicket, SkillDungeonTicket }

public enum HUDType { Shop, Stat, Skill, Inventory, Menu }

public enum LanguageType { KR, EN }

public enum StageType { Forest, Cave }

public enum SkillId { None, Lightning, DarkBoom, HolyBurst, DragonBreath, IceArrow, Explosion, Fireball, Charge }

public enum ItemType { Weapon, Armor, Necklace, Consumables }

public enum GradeType { Common, Uncommon, Rare, Epic, Legendary, Mythical }

public enum SkillType { Active, Buff, Passive }

public enum SkillEffectType { GoldBonus, ExpBonus, DamageBonus, HealthBonus, CriticalDamageBonus, CriticalChanceBonus }

public enum StatusEffectType { None, ElectricShock, Burn, Frozen, Stun }

public enum ProjectileId { Lightning, DarkBoom, DragonBreath, IceArrow, Explosion, Charge, Fireball }

public enum ParticleId { ClickEffect, Lightning, DarkBoom, HolyBurst, DragonBreath, IceArrow, Explosion, Fireball, ChargeLoop, ChargeHit, }

public enum ShopCategory { Summon, Normal, Skill, Score, Package, Cash }

public enum SummonSubCategory { Weapon, Armor, Necklace, Skill }

public enum RewardType { Item, Diamond, EnhanceStone, SkillGem }

public enum StateType { None, Idle, Attack, Chase, Dead, Charge }

public enum ShopRewardType { AdRemove, EnhanceDungeonTicket, SkillDungeonTicket, EnhanceStone, SkillGem }

public enum ShopPriceType { Diamond, SkillGem, Ad, Cash }

public enum ShopLimitType { None, Daily, Weekly, Monthly, Account }

public enum DungeonType { None = -1, EnhanceDungeon, SkillDungeon }
