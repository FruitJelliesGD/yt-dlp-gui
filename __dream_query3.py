import sqlite3
import json

DB = r"C:\Users\23277\.local\share\mimocode\mimocode.db"
conn = sqlite3.connect(DB)
conn.row_factory = sqlite3.Row
c = conn.cursor()

import sys
action = sys.argv[1]

if action == "sample_user_msg":
    # Get a sample user message to understand data format
    c.execute("""
        SELECT m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.session_id = 'ses_09dc67d03ffe6REGqS1mth4VLM'
        ORDER BY m.time_created
        LIMIT 3
    """)
    for r in c.fetchall():
        print(f"ID: {r['id']}")
        data = json.loads(r['data'])
        print(json.dumps(data, indent=2, ensure_ascii=False)[:2000])
        print("---")

elif action == "user_text":
    # Try to extract user text from content array format
    c.execute("""
        SELECT m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.session_id = 'ses_09dc67d03ffe6REGqS1mth4VLM'
        ORDER BY m.time_created
    """)
    for r in c.fetchall():
        data = json.loads(r['data'])
        content = data.get('content', [])
        if isinstance(content, list):
            for item in content:
                if isinstance(item, dict) and item.get('type') == 'text':
                    text = item.get('text', '')
                    if text.strip():
                        print(f"[{r['id']}] {text[:500]}")
                        print()
        elif isinstance(content, str) and content.strip():
            print(f"[{r['id']}] {content[:500]}")
            print()

elif action == "all_user_text":
    # Get all user text from all non-checkpoint sessions
    c.execute("""
        SELECT m.session_id, m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.session_id IN (
            SELECT id FROM session WHERE parent_id IS NULL
              AND project_id = '8f4dbf3c-e726-439b-9e54-4fee8d8cfc56'
          )
        ORDER BY m.time_created DESC
        LIMIT 100
    """)
    for r in c.fetchall():
        data = json.loads(r['data'])
        content = data.get('content', [])
        texts = []
        if isinstance(content, list):
            for item in content:
                if isinstance(item, dict) and item.get('type') == 'text':
                    texts.append(item.get('text', ''))
        elif isinstance(content, str):
            texts.append(content)
        for text in texts:
            if text.strip():
                print(f"[{r['session_id']}] {text[:500]}")
                print()

conn.close()
