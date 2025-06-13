public enum EnemyType { None = -1, Melee = 0, Range = 1, Boss = 2 } //None = -1 : ����Ʈ������ ����ϱ� ����, ���߿� ������ �ȵ� ���� üũ�� �� ������.

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