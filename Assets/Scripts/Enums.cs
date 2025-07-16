public enum EnemyType { None = -1, Melee = 0, Range = 1, Boss = 2 } //None = -1 : 디폴트값으로 사용하기 좋음, 나중에 설정이 안된 상태 체크할 때 유용함.

public enum EnemyId { Skeleton_Knight, Skeleton_Archer, Skeleton_Warrior, Skeleton_Shield }

public enum StatUpgradeType { Attack, Health, AttackSpeed, MoveSpeed, }

public enum GoldUpgradeType { Attack, Health, CriticalChance, CriticalDamage }

public enum PlayerProgressType { Level, CurrentExp, MaxExp, StatPoint, Gold, Diamond, EnhanceStone }

public enum HUDType { Shop, Stat, Skill, Inventory, Menu }

public enum LanguageType { KR, EN }

public enum StageType { Forest, Cave }

public enum SkillId { None, Lightning, DarkBoom, HolyBurst, DragonBreath, IceArrow }

public enum ItemType { Weapon, Armor, Necklace }

public enum GradeType { Common, Uncommon, Rare, Epic, Legendary, Mythical }

public enum SkillType { Active, Buff, Passive }

public enum SkillEffectType { GoldBonus, ExpBonus, DamageBonus, HealthBonus, CriticalDamageBonus, CriticalChanceBonus }

public enum StatusEffectType { None, ElectricShock, Burn, Frozen }

public enum ProjectileId { Lightning, DarkBoom, DragonBreath, IceArrow }

public enum ParticleId { ClickEffect, Lightning, DarkBoom, HolyBurst, DragonBreath, IceArrow }

public enum ShopCategory { Summon, Normal, Skill, Score, Package, Cash }

public enum SummonSubCategory { Weapon, Armor, Necklace, Skill }

public enum RewardType { Item, Currency }

public enum StateType { None, Idle, Attack, Chase, Dead }