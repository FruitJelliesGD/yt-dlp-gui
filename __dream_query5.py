import sqlite3
import json

DB = r"C:\Users\23277\.local\share\mimocode\mimocode.db"
conn = sqlite3.connect(DB)
conn.row_factory = sqlite3.Row
c = conn.cursor()

import sys
action = sys.argv[1]

if action == "other_projects":
    # Sessions from other projects
    c.execute("""
        SELECT DISTINCT s.project_id, s.id, s.title, s.directory
        FROM session s
        WHERE s.project_id != '8f4dbf3c-e726-439b-9e54-4fee8d8cfc56'
          AND s.parent_id IS NULL
        ORDER BY s.time_created DESC
        LIMIT 10
    """)
    for r in c.fetchall():
        print(dict(r))

elif action == "recent_task_events":
    # Task events for the main session
    c.execute("""
        SELECT * FROM task_event
        WHERE session_id = 'ses_09dc67d03ffe6REGqS1mth4VLM'
        ORDER BY time_created DESC
        LIMIT 20
    """)
    for r in c.fetchall():
        print(dict(r))

elif action == "recent_tasks":
    # Tasks for the main session
    c.execute("""
        SELECT * FROM task
        WHERE session_id = 'ses_09dc67d03ffe6REGqS1mth4VLM'
        ORDER BY time_created
    """)
    for r in c.fetchall():
        d = dict(r)
        # Truncate data field
        if 'data' in d:
            d['data'] = d['data'][:200]
        print(d)

elif action == "user_preferences_search":
    # Search for common preference patterns in user messages
    c.execute("""
        SELECT m.session_id, m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
        ORDER BY m.time_created DESC
    """)
    pref_keywords = ['prefer', 'always', 'never', 'remember', 'rule', 'style', 'language', 'chinese', '中文']
    for r in c.fetchall():
        data = json.loads(r['data'])
        # Check all text content
        content = data.get('content', [])
        if isinstance(content, list):
            for item in content:
                if isinstance(item, dict) and item.get('type') == 'text':
                    text = item.get('text', '').lower()
                    for kw in pref_keywords:
                        if kw in text:
                            print(f"[{r['session_id']}] {item.get('text', '')[:500]}")
                            print()
                            break

conn.close()
