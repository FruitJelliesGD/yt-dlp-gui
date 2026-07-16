import sqlite3
import json

DB = r"C:\Users\23277\.local\share\mimocode\mimocode.db"
conn = sqlite3.connect(DB)
conn.row_factory = sqlite3.Row
c = conn.cursor()

import sys
action = sys.argv[1] if len(sys.argv) > 1 else "user_content"

if action == "user_content":
    # Get all user messages from main session with full content
    sid = sys.argv[2] if len(sys.argv) > 2 else "ses_09dc67d03ffe6REGqS1mth4VLM"
    c.execute("""
        SELECT m.id, json_extract(m.data, '$.content') as content
        FROM message m
        WHERE m.session_id = ?
          AND json_extract(m.data, '$.role') = 'user'
        ORDER BY m.time_created
    """, (sid,))
    for r in c.fetchall():
        content = r['content']
        if content:
            print(f"--- {r['id']} ---")
            print(content[:1000])
            print()

elif action == "user_content_all_sessions":
    # Get all user messages from all non-checkpoint-writer sessions
    c.execute("""
        SELECT m.session_id, m.id, substr(json_extract(m.data, '$.content'), 1, 500) as content
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.session_id NOT IN (
            SELECT id FROM session WHERE title LIKE 'checkpoint-writer%'
          )
        ORDER BY m.time_created DESC
        LIMIT 50
    """)
    for r in c.fetchall():
        content = r['content']
        if content:
            print(f"[{r['session_id']}] {r['id']}: {content[:300]}")
            print()

elif action == "other_checkpoints":
    # Read all non-main session checkpoints
    c.execute("""
        SELECT id, title, time_created FROM session 
        WHERE project_id = '8f4dbf3c-e726-439b-9e54-4fee8d8cfc56'
          AND id != 'ses_09dc67d03ffe6REGqS1mth4VLM'
          AND parent_id IS NULL
        ORDER BY time_created DESC
        LIMIT 10
    """)
    for r in c.fetchall():
        print(dict(r))

elif action == "parent_sessions":
    # Get main sessions (no parent)
    c.execute("""
        SELECT id, title, time_created, parent_id FROM session 
        WHERE project_id = '8f4dbf3c-e726-439b-9e54-4fee8d8cfc56'
          AND parent_id IS NULL
        ORDER BY time_created DESC
    """)
    for r in c.fetchall():
        print(dict(r))

conn.close()
