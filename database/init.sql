PRAGMA journal_mode=WAL;
PRAGMA synchronous=NORMAL;
PRAGMA busy_timeout=5000;

CREATE TABLE IF NOT EXISTS sculptures (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    location TEXT NOT NULL,
    image_url TEXT NOT NULL,
    x_position REAL DEFAULT 0,
    y_position REAL DEFAULT 0,
    status TEXT DEFAULT 'normal' CHECK (status IN ('normal', 'warning', 'alert')),
    salt_coverage REAL DEFAULT 0,
    base_strength REAL DEFAULT 2.0,
    description TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS sensors (
    id TEXT PRIMARY KEY,
    sculpture_id INTEGER NOT NULL REFERENCES sculptures(id),
    sensor_type TEXT NOT NULL CHECK (sensor_type IN ('ion', 'environment')),
    model TEXT,
    location_description TEXT,
    install_x REAL DEFAULT 0,
    install_y REAL DEFAULT 0,
    install_z REAL DEFAULT 0,
    is_active BOOLEAN DEFAULT 1,
    last_report_time DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS sensor_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sensor_id TEXT NOT NULL REFERENCES sensors(id),
    sculpture_id INTEGER NOT NULL REFERENCES sculptures(id),
    timestamp DATETIME NOT NULL,
    data_type TEXT NOT NULL CHECK (data_type IN ('ion', 'environment')),
    na_concentration REAL,
    k_concentration REAL,
    ca_concentration REAL,
    temperature REAL,
    humidity REAL,
    signal_strength REAL,
    battery_level INTEGER
);

CREATE INDEX IF NOT EXISTS idx_sensor_data_sculpture_time ON sensor_data(sculpture_id, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_sensor_data_sensor_time ON sensor_data(sensor_id, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_sensor_data_type_time ON sensor_data(data_type, timestamp DESC);

CREATE TABLE IF NOT EXISTS alerts (
    id TEXT PRIMARY KEY,
    sculpture_id INTEGER NOT NULL REFERENCES sculptures(id),
    sensor_id TEXT REFERENCES sensors(id),
    alert_type TEXT NOT NULL CHECK (alert_type IN ('salt_coverage', 'na_high', 'k_high', 'ca_high', 'sensor_offline')),
    severity TEXT NOT NULL CHECK (severity IN ('low', 'medium', 'high')),
    threshold_value REAL NOT NULL,
    current_value REAL NOT NULL,
    message TEXT NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    acknowledged BOOLEAN DEFAULT 0,
    acknowledged_at DATETIME,
    acknowledged_by INTEGER REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS idx_alerts_sculpture_time ON alerts(sculpture_id, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_alerts_severity ON alerts(severity, timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_alerts_acknowledged ON alerts(acknowledged, timestamp DESC);

CREATE TABLE IF NOT EXISTS materials (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    manufacturer TEXT,
    description TEXT,
    default_contact_angle REAL NOT NULL,
    default_penetration_depth REAL NOT NULL,
    default_strength REAL NOT NULL,
    weather_resistance REAL DEFAULT 85,
    reversibility REAL DEFAULT 70,
    cost_per_kg REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS material_scores (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sculpture_id INTEGER NOT NULL REFERENCES sculptures(id),
    material_id TEXT NOT NULL REFERENCES materials(id),
    contact_angle_score REAL NOT NULL,
    penetration_depth_score REAL NOT NULL,
    strength_match_score REAL NOT NULL,
    weather_resistance_score REAL NOT NULL,
    reversibility_score REAL NOT NULL,
    cost_performance_score REAL NOT NULL,
    total_score REAL NOT NULL,
    recommendation TEXT,
    calculated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL DEFAULT 'viewer' CHECK (role IN ('admin', 'conservator', 'viewer')),
    email TEXT,
    phone TEXT,
    is_active BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS alert_thresholds (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    alert_type TEXT UNIQUE NOT NULL,
    warning_threshold REAL NOT NULL,
    critical_threshold REAL NOT NULL,
    unit TEXT NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS dingtalk_configs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    webhook_url TEXT,
    secret TEXT,
    at_mobiles TEXT,
    is_at_all BOOLEAN DEFAULT 0,
    enabled BOOLEAN DEFAULT 0,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS migration_predictions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    sculpture_id INTEGER NOT NULL REFERENCES sculptures(id),
    porosity REAL NOT NULL,
    saturation REAL NOT NULL,
    temperature REAL NOT NULL,
    humidity REAL NOT NULL,
    surface_concentration REAL NOT NULL,
    prediction_hours INTEGER NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS migration_prediction_points (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    prediction_id INTEGER NOT NULL REFERENCES migration_predictions(id),
    time_hour INTEGER NOT NULL,
    depth_cm REAL NOT NULL,
    concentration REAL NOT NULL
);

INSERT INTO alert_thresholds (alert_type, warning_threshold, critical_threshold, unit, description) VALUES
('salt_coverage', 20.0, 30.0, '%', '表面盐结晶覆盖率'),
('na_high', 300.0, 500.0, 'ppm', '钠离子浓度'),
('k_high', 300.0, 500.0, 'ppm', '钾离子浓度'),
('ca_high', 300.0, 500.0, 'ppm', '钙离子浓度'),
('sensor_offline', 90.0, 180.0, 'minutes', '传感器离线时长');

INSERT INTO materials (id, name, manufacturer, description, default_contact_angle, default_penetration_depth, default_strength, weather_resistance, reversibility, cost_per_kg) VALUES
('TEOS-001', '正硅酸乙酯(TEOS)', 'Sigma-Aldrich', '硅酸盐类加固材料，渗透性好，固化后形成SiO2网络', 65.0, 5.0, 2.5, 95.0, 40.0, 350.0),
('NANO-LIME-001', '纳米石灰乳液', 'Custom Materials', '氢氧化钙纳米颗粒，与基体兼容性好，可碳化强化', 55.0, 8.0, 1.8, 88.0, 90.0, 280.0),
('ACRYLIC-001', '丙烯酸树脂', 'BASF', '有机聚合物类，成膜性好，强度高', 75.0, 3.0, 3.5, 82.0, 30.0, 180.0),
('SILANE-001', '硅烷偶联剂', 'Dow Corning', '有机硅类，兼具防水和加固功能，小分子易渗透', 85.0, 6.0, 3.0, 90.0, 50.0, 420.0);

INSERT INTO users (username, password_hash, role, email, phone, is_active) VALUES
('admin', 'AQAAAAIAAYagAAAAEJx1r3q7LJ8eUQaW5b6c7d8e9f0a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6', 'admin', 'admin@culturalheritage.org', '13800138000', 1),
('conservator', 'AQAAAAIAAYagAAAAEKwV1b5c8d2e9f3a7b4c6d8e0f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8a0b2', 'conservator', 'conservator@culturalheritage.org', '13900139000', 1);

INSERT INTO sculptures (id, name, location, image_url, x_position, y_position, status, salt_coverage, base_strength, description) VALUES
(1, '释迦牟尼佛', '大雄宝殿-中央', '/images/sculptures/001.jpg', 50.0, 20.0, 'normal', 5.2, 2.1, '主尊造像，明代永乐年间，铜骨泥胎，敷金彩绘'),
(2, '文殊菩萨', '大雄宝殿-左一', '/images/sculptures/002.jpg', 30.0, 20.0, 'normal', 8.3, 1.9, '胁侍菩萨，骑青狮，手持智慧剑'),
(3, '普贤菩萨', '大雄宝殿-右一', '/images/sculptures/003.jpg', 70.0, 20.0, 'warning', 22.5, 1.8, '胁侍菩萨，骑白象，手持莲花，表面盐析明显'),
(4, '观音菩萨', '大雄宝殿-左二', '/images/sculptures/004.jpg', 20.0, 35.0, 'normal', 6.8, 2.0, '大悲观音，千手千眼造型'),
(5, '地藏菩萨', '大雄宝殿-右二', '/images/sculptures/005.jpg', 80.0, 35.0, 'normal', 9.1, 1.7, '地藏王，手持锡杖，安忍不动'),
(6, '弥勒菩萨', '大雄宝殿-左三', '/images/sculptures/006.jpg', 15.0, 50.0, 'alert', 35.6, 1.5, '未来佛，笑口常开，底部盐害严重'),
(7, '韦陀菩萨', '大雄宝殿-右三', '/images/sculptures/007.jpg', 85.0, 50.0, 'normal', 7.4, 2.2, '护法韦陀，手持金刚杵，威风凛凛'),
(8, '伽蓝菩萨', '大雄宝殿-左四', '/images/sculptures/008.jpg', 10.0, 65.0, 'normal', 8.9, 2.0, '伽蓝护法，关公造像，忠义千秋'),
(9, '大势至菩萨', '大雄宝殿-右四', '/images/sculptures/009.jpg', 90.0, 65.0, 'warning', 25.3, 1.6, '大势至，智慧之光，普照众生'),
(10, '药王菩萨', '大雄宝殿-左五', '/images/sculptures/010.jpg', 5.0, 80.0, 'normal', 4.5, 2.1, '药王，施医给药，救度众生'),
(11, '药上菩萨', '大雄宝殿-右五', '/images/sculptures/011.jpg', 95.0, 80.0, 'normal', 6.2, 1.9, '药上，法药医心，拔除业障'),
(12, '阿难尊者', '大雄宝殿-佛左', '/images/sculptures/012.jpg', 40.0, 30.0, 'normal', 7.8, 2.0, '佛陀侍者，多闻第一'),
(13, '迦叶尊者', '大雄宝殿-佛右', '/images/sculptures/013.jpg', 60.0, 30.0, 'normal', 5.9, 2.1, '佛陀侍者，头陀第一'),
(14, '舍利弗尊者', '大雄宝殿-前左一', '/images/sculptures/014.jpg', 35.0, 55.0, 'normal', 6.5, 2.0, '智慧第一'),
(15, '目犍连尊者', '大雄宝殿-前右一', '/images/sculptures/015.jpg', 65.0, 55.0, 'warning', 21.8, 1.7, '神通第一'),
(16, '须菩提尊者', '大雄宝殿-前左二', '/images/sculptures/016.jpg', 25.0, 70.0, 'normal', 4.8, 2.2, '解空第一'),
(17, '富楼那尊者', '大雄宝殿-前右二', '/images/sculptures/017.jpg', 75.0, 70.0, 'normal', 5.6, 1.9, '说法第一'),
(18, '摩诃迦旃延', '大雄宝殿-前左三', '/images/sculptures/018.jpg', 20.0, 85.0, 'normal', 6.1, 2.0, '论议第一'),
(19, '优婆离尊者', '大雄宝殿-前右三', '/images/sculptures/019.jpg', 80.0, 85.0, 'normal', 7.2, 1.8, '持律第一'),
(20, '罗睺罗尊者', '大雄宝殿-前左四', '/images/sculptures/020.jpg', 15.0, 95.0, 'normal', 5.4, 2.1, '密行第一'),
(21, '阿那律尊者', '大雄宝殿-前右四', '/images/sculptures/021.jpg', 85.0, 95.0, 'alert', 42.1, 1.4, '天眼第一，表面严重泛碱'),
(22, '难陀尊者', '后殿-左一', '/images/sculptures/022.jpg', 20.0, 15.0, 'normal', 6.7, 2.0, '佛陀堂弟，仪容第一'),
(23, '孙陀罗难陀', '后殿-右一', '/images/sculptures/023.jpg', 80.0, 15.0, 'normal', 5.8, 1.9, '比丘，精进修行'),
(24, '摩诃男', '后殿-左二', '/images/sculptures/024.jpg', 15.0, 30.0, 'warning', 27.4, 1.6, '释迦族，王族出家'),
(25, '阿鞞跋致', '后殿-右二', '/images/sculptures/025.jpg', 85.0, 30.0, 'normal', 4.9, 2.1, '不退转菩萨'),
(26, '常不轻菩萨', '后殿-左三', '/images/sculptures/026.jpg', 10.0, 45.0, 'normal', 6.3, 2.0, '礼拜一切众生'),
(27, '药王菩萨', '后殿-右三', '/images/sculptures/027.jpg', 90.0, 45.0, 'normal', 5.5, 1.9, '另尊药王造像'),
(28, '无尽意菩萨', '后殿-左四', '/images/sculptures/028.jpg', 5.0, 60.0, 'warning', 23.6, 1.7, '东方不思议世界菩萨'),
(29, '金刚藏菩萨', '后殿-右四', '/images/sculptures/029.jpg', 95.0, 60.0, 'normal', 7.1, 2.0, '金刚界菩萨'),
(30, '韦陀护法', '山门-右侧', '/images/sculptures/030.jpg', 80.0, 80.0, 'normal', 7.8, 2.1, '护法神将，明代风格，守护寺门');

INSERT INTO sensors (id, sculpture_id, sensor_type, model, location_description, install_x, install_y, install_z, is_active) VALUES
('ION-001-01', 1, 'ion', 'IonSensor-Pro-200', '佛身左侧中部', 30.0, 50.0, 20.0, 1),
('ION-001-02', 1, 'ion', 'IonSensor-Pro-200', '佛身右侧中部', 70.0, 50.0, 20.0, 1),
('ION-002-01', 2, 'ion', 'IonSensor-Pro-200', '菩萨身左侧', 35.0, 55.0, 18.0, 1),
('ION-002-02', 2, 'ion', 'IonSensor-Pro-200', '狮身右侧', 65.0, 75.0, 10.0, 1),
('ION-003-01', 3, 'ion', 'IonSensor-Pro-200', '菩萨身中部', 50.0, 50.0, 18.0, 1),
('ION-003-02', 3, 'ion', 'IonSensor-Pro-200', '象身左侧', 35.0, 75.0, 10.0, 1),
('ION-004-01', 4, 'ion', 'IonSensor-Pro-200', '主手左侧', 40.0, 45.0, 22.0, 1),
('ION-004-02', 4, 'ion', 'IonSensor-Pro-200', '主手右侧', 60.0, 45.0, 22.0, 1),
('ION-005-01', 5, 'ion', 'IonSensor-Pro-200', '锡杖左侧', 45.0, 40.0, 15.0, 1),
('ION-005-02', 5, 'ion', 'IonSensor-Pro-200', '锡杖右侧', 55.0, 40.0, 15.0, 1),
('ION-006-01', 6, 'ion', 'IonSensor-Pro-200', '佛身左侧', 35.0, 50.0, 20.0, 1),
('ION-006-02', 6, 'ion', 'IonSensor-Pro-200', '佛座中部', 50.0, 85.0, 5.0, 1),
('ION-007-01', 7, 'ion', 'IonSensor-Pro-200', '金刚杵左侧', 40.0, 35.0, 18.0, 1),
('ION-007-02', 7, 'ion', 'IonSensor-Pro-200', '金刚杵右侧', 60.0, 35.0, 18.0, 1),
('ION-008-01', 8, 'ion', 'IonSensor-Pro-200', '青龙偃月刀左侧', 38.0, 45.0, 16.0, 1),
('ION-008-02', 8, 'ion', 'IonSensor-Pro-200', '青龙偃月刀右侧', 62.0, 45.0, 16.0, 1),
('ION-009-01', 9, 'ion', 'IonSensor-Pro-200', '莲花左侧', 42.0, 48.0, 19.0, 1),
('ION-009-02', 9, 'ion', 'IonSensor-Pro-200', '莲花右侧', 58.0, 48.0, 19.0, 1),
('ION-010-01', 10, 'ion', 'IonSensor-Pro-200', '药壶左侧', 40.0, 52.0, 17.0, 1),
('ION-010-02', 10, 'ion', 'IonSensor-Pro-200', '药壶右侧', 60.0, 52.0, 17.0, 1),
('ION-011-01', 11, 'ion', 'IonSensor-Pro-200', '身左侧', 35.0, 50.0, 18.0, 1),
('ION-011-02', 11, 'ion', 'IonSensor-Pro-200', '身右侧', 65.0, 50.0, 18.0, 1),
('ION-012-01', 12, 'ion', 'IonSensor-Pro-200', '手持经卷左侧', 45.0, 55.0, 15.0, 1),
('ION-013-01', 13, 'ion', 'IonSensor-Pro-200', '身右侧', 55.0, 55.0, 15.0, 1),
('ION-014-01', 14, 'ion', 'IonSensor-Pro-200', '头部左侧', 40.0, 30.0, 12.0, 1),
('ION-014-02', 14, 'ion', 'IonSensor-Pro-200', '身右侧', 60.0, 55.0, 15.0, 1),
('ION-015-01', 15, 'ion', 'IonSensor-Pro-200', '身中部', 50.0, 50.0, 16.0, 1),
('ION-016-01', 16, 'ion', 'IonSensor-Pro-200', '身左侧', 35.0, 48.0, 14.0, 1),
('ION-016-02', 16, 'ion', 'IonSensor-Pro-200', '身右侧', 65.0, 48.0, 14.0, 1),
('ION-017-01', 17, 'ion', 'IonSensor-Pro-200', '口部前方', 50.0, 40.0, 18.0, 1),
('ION-018-01', 18, 'ion', 'IonSensor-Pro-200', '身左侧', 38.0, 50.0, 16.0, 1),
('ION-019-01', 19, 'ion', 'IonSensor-Pro-200', '持律手左侧', 42.0, 52.0, 15.0, 1),
('ION-020-01', 20, 'ion', 'IonSensor-Pro-200', '身左侧', 35.0, 55.0, 14.0, 1),
('ION-021-01', 21, 'ion', 'IonSensor-Pro-200', '眉间第三眼', 50.0, 25.0, 12.0, 1),
('ION-021-02', 21, 'ion', 'IonSensor-Pro-200', '底座', 50.0, 90.0, 5.0, 1),
('ION-022-01', 22, 'ion', 'IonSensor-Pro-200', '身左侧', 35.0, 50.0, 15.0, 1),
('ION-023-01', 23, 'ion', 'IonSensor-Pro-200', '身右侧', 65.0, 50.0, 15.0, 1),
('ION-024-01', 24, 'ion', 'IonSensor-Pro-200', '身中部', 50.0, 50.0, 16.0, 1),
('ION-025-01', 25, 'ion', 'IonSensor-Pro-200', '身左侧', 35.0, 50.0, 18.0, 1),
('ION-030-01', 30, 'ion', 'IonSensor-Pro-200', '护法身中部', 50.0, 45.0, 15.0, 1);

INSERT INTO sensors (id, sculpture_id, sensor_type, model, location_description, install_x, install_y, install_z, is_active) VALUES
('ENV-001-01', 1, 'environment', 'EnvSensor-HT-500', '佛座左侧', 25.0, 15.0, 10.0, 1),
('ENV-002-01', 2, 'environment', 'EnvSensor-HT-500', '狮座左侧', 25.0, 85.0, 8.0, 1),
('ENV-003-01', 3, 'environment', 'EnvSensor-HT-500', '象座左侧', 25.0, 85.0, 8.0, 1),
('ENV-004-01', 4, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-005-01', 5, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-006-01', 6, 'environment', 'EnvSensor-HT-500', '底座中部', 50.0, 95.0, 5.0, 1),
('ENV-007-01', 7, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-008-01', 8, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-009-01', 9, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-010-01', 10, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-011-01', 11, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-012-01', 12, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-013-01', 13, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-014-01', 14, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-015-01', 15, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-016-01', 16, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-017-01', 17, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-018-01', 18, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-019-01', 19, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-020-01', 20, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-021-01', 21, 'environment', 'EnvSensor-HT-500', '底座中部', 50.0, 95.0, 5.0, 1),
('ENV-022-01', 22, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-023-01', 23, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-024-01', 24, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-025-01', 25, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-026-01', 26, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-027-01', 27, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-028-01', 28, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-029-01', 29, 'environment', 'EnvSensor-HT-500', '底座左侧', 25.0, 85.0, 8.0, 1),
('ENV-030-01', 30, 'environment', 'EnvSensor-HT-500', '像座左侧', 25.0, 85.0, 8.0, 1);

INSERT INTO dingtalk_configs (webhook_url, secret, at_mobiles, is_at_all, enabled) VALUES
('', '', '13800138000', 0, 0);

CREATE TRIGGER IF NOT EXISTS update_sculptures_timestamp
AFTER UPDATE ON sculptures
FOR EACH ROW
BEGIN
    UPDATE sculptures SET updated_at = CURRENT_TIMESTAMP WHERE id = OLD.id;
END;

CREATE TRIGGER IF NOT EXISTS update_dingtalk_timestamp
AFTER UPDATE ON dingtalk_configs
FOR EACH ROW
BEGIN
    UPDATE dingtalk_configs SET updated_at = CURRENT_TIMESTAMP WHERE id = OLD.id;
END;
