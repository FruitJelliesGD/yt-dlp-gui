import sqlite3
import json

DB = r"C:\Users\23277\.local\share\mimocode\mimocode.db"
conn = sqlite3.connect(DB)
conn.row_factory = sqlite3.Row
c = conn.cursor()

import sys
action = sys.argv[1]

if action == "all_user_data_keys":
    # Look at all unique keys in user message data
    c.execute("""
        SELECT m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.session_id = 'ses_09dc67d03ffe6REGqS1mth4VLM'
        ORDER BY m.time_created
        LIMIT 5
    """)
    for r in c.fetchall():
        data = json.loads(r['data'])
        print(json.dumps(list(data.keys()), ensure_ascii=False))

elif action == "user_text_in_compose":
    # Search for user messages with actual text content in compose sessions
    c.execute("""
        SELECT m.session_id, m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.agent_id = 'main'
        ORDER BY m.time_created
        LIMIT 20
    """)
    for r in c.fetchall():
        data = json.loads(r['data'])
        # Check all string fields for text
        content = data.get('content')
        text = data.get('text')
        msg = data.get('message')
        summary = data.get('summary', {})
        print(f"Keys: {list(data.keys())}")
        if content:
            print(f"  content: {str(content)[:300]}")
        if text:
            print(f"  text: {str(text)[:300]}")
        if msg:
            print(f"  message: {str(msg)[:300]}")
        if summary:
            diffs = summary.get('diffs', [])
            if diffs:
                print(f"  summary.diffs: {len(diffs)} files")
                for d in diffs[:3]:
                    print(f"    - {d.get('file', 'unknown')}")
        print()

elif action == "non_compose_users":
    # Get user messages from non-compose agents (general agent sessions)
    c.execute("""
        SELECT m.session_id, m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.agent_id != 'main'
          AND m.session_id IN (
            SELECT id FROM session WHERE parent_id IS NULL
              AND project_id = '8f4dbf3c-e726-439b-9e54-4fee8d8cfc56'
          )
        ORDER BY m.time_created
        LIMIT 30
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

elif action == "compose_user_summaries":
    # Look at the summary diffs for compose user messages to understand what user chose
    c.execute("""
        SELECT m.session_id, m.id, m.data
        FROM message m
        WHERE json_extract(m.data, '$.role') = 'user'
          AND m.agent_id = 'main'
        ORDER BY m.time_created
    """)
    for r in c.fetchall():
        data = json.loads(r['data'])
        summary = data.get('summary', {})
        diffs = summary.get('diffs', [])
        if diffs:
            files = [d.get('file', '?') for d in diffs]
            # Only show non-.compose files
            real_files = [f for f in files if not f.startswith('.compose/')]
            if real_files:
                print(f"[{r['session_id']}] {r['id']}:")
                for f in real_files[:10]:
                    print(f"  {f}")
                print()

conn.close()
