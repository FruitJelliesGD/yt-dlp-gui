import sqlite3
import json
import sys

DB = r"C:\Users\23277\.local\share\mimocode\mimocode.db"
conn = sqlite3.connect(DB)
conn.row_factory = sqlite3.Row
c = conn.cursor()

action = sys.argv[1] if len(sys.argv) > 1 else "tables"

if action == "tables":
    c.execute("SELECT name FROM sqlite_master WHERE type='table'")
    for r in c.fetchall():
        print(r[0])

elif action == "schema":
    table = sys.argv[2]
    c.execute(f"SELECT sql FROM sqlite_master WHERE name='{table}'")
    row = c.fetchone()
    print(row[0] if row else "not found")

elif action == "sessions":
    # List sessions with metadata, newest first
    c.execute("SELECT * FROM session ORDER BY time_created DESC LIMIT 20")
    for r in c.fetchall():
        print(dict(r))

elif action == "project_sessions":
    # Sessions for this project (yt-dlp-gui)
    c.execute("""
        SELECT s.* FROM session s 
        WHERE s.directory LIKE '%yt-dlp-gui%'
        ORDER BY s.time_created DESC LIMIT 20
    """)
    for r in c.fetchall():
        print(dict(r))

elif action == "session_messages":
    sid = sys.argv[2]
    c.execute("""
        SELECT m.id, m.agent_id, json_extract(m.data, '$.role') as role, 
               substr(m.data, 1, 200) as preview
        FROM message m
        WHERE m.session_id = ?
        ORDER BY m.time_created
    """, (sid,))
    for r in c.fetchall():
        print(dict(r))

elif action == "session_parts":
    sid = sys.argv[2]
    c.execute("""
        SELECT m.id as msg_id, m.agent_id,
               json_extract(p.data, '$.type') as part_type,
               json_extract(p.data, '$.tool') as tool,
               substr(p.data, 1, 800) as preview
        FROM message m
        JOIN part p ON p.message_id = m.id
        WHERE m.session_id = ?
          AND json_extract(m.data, '$.role') = 'assistant'
        ORDER BY m.time_created, p.time_created
    """, (sid,))
    for r in c.fetchall():
        print(dict(r))

elif action == "user_statements":
    # Search for user messages containing keywords
    keyword = sys.argv[2]
    c.execute("""
        SELECT m.session_id, m.id, substr(json_extract(m.data, '$.content'), 1, 500) as content
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND json_extract(m.data, '$.content') LIKE ?
        ORDER BY m.time_created DESC
        LIMIT 30
    """, (f"%{keyword}%",))
    for r in c.fetchall():
        print(dict(r))

elif action == "tasks":
    sid = sys.argv[2] if len(sys.argv) > 2 else None
    if sid:
        c.execute("SELECT * FROM task WHERE session_id = ? ORDER BY time_created", (sid,))
    else:
        c.execute("SELECT * FROM task ORDER BY time_created DESC LIMIT 30")
    for r in c.fetchall():
        print(dict(r))

conn.close()
