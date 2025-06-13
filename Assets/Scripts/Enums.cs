public enum EnemyType { None = -1, Melee = 0, Range = 1, Boss = 2 } //None = -1 : 디폴트값으로 사용하기 좋음, 나중에 설정이 안된 상태 체크할 때 유용함.

public enum EnemyId { Skeleton, Slime, Bat, Goblin }

public enum StatType { Attack, Health, Critical, AttackSpeed, MoveSpeed, }

public enum PlayerProgressStat { Level, CurrentExp, MaxExp, StatPoint, Gold, }

public enum HUDType { Shop, Stat, Skill, Inventory, Menu }

public enum LanguageType { KR, EN }

public enum StageType { Forest, Cave }

public enum SkillId
{
    Lightning,
}

public enum GradeType { Common, Uncommon, Rare, Epic, Legendary, Mythical }

public enum SkillType { Active, Buff, Passive }

public enum SkillEffectType { GoldBonus, ExpBonus }

public enum StatusEffectType { ElectricShock }