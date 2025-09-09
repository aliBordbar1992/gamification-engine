import React, { useEffect, useRef, useState } from 'react'
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api'
import 'monaco-editor/esm/vs/editor/contrib/suggest/browser/suggestController'
import 'monaco-editor/esm/vs/editor/contrib/suggest/browser/suggestWidget'
import 'monaco-editor/esm/vs/language/json/monaco.contribution'
import EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker?worker'
import JsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker'
import CssWorker from 'monaco-editor/esm/vs/language/css/css.worker?worker'
import HtmlWorker from 'monaco-editor/esm/vs/language/html/html.worker?worker'
import TsWorker from 'monaco-editor/esm/vs/language/typescript/ts.worker?worker'

export interface MonacoEditorProps {
  value: string
  onChange: (value: string) => void
  language?: 'json' | 'yaml'
  height?: number
  readOnly?: boolean
  placeholder?: string
  schema?: Record<string, unknown>
  onValidationChange?: (
    isValid: boolean,
    errors: monaco.editor.IMarker[]
  ) => void
  userIdOptions?: string[]
}

const MonacoEditor: React.FC<MonacoEditorProps> = ({
  value,
  onChange,
  language = 'json',
  height = 400,
  readOnly = false,
  schema,
  onValidationChange,
  userIdOptions = [],
}) => {
  const editorRef = useRef<HTMLDivElement>(null)
  const monacoEditorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(
    null
  )
  const [isEditorReady, setIsEditorReady] = useState(false)
  const onChangeRef = useRef(onChange)
  useEffect(() => {
    onChangeRef.current = onChange
  }, [onChange])

  // Create editor once on mount
  useEffect(() => {
    if (!editorRef.current) return

    // Configure Monaco environment for web workers (Vite module workers)
    if (typeof window !== 'undefined') {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      ;(window as any).MonacoEnvironment = {
        getWorker: function (_workerId: string, label: string) {
          if (label === 'json') {
            return new JsonWorker()
          }
          if (label === 'css' || label === 'scss' || label === 'less') {
            return new CssWorker()
          }
          if (label === 'html' || label === 'handlebars' || label === 'razor') {
            return new HtmlWorker()
          }
          if (label === 'typescript' || label === 'javascript') {
            return new TsWorker()
          }
          return new EditorWorker()
        },
      }
    }

    // Create editor instance
    const editor = monaco.editor.create(editorRef.current, {
      value: value || '',
      language: language,
      theme: 'vs-dark',
      automaticLayout: true,
      readOnly: readOnly,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      wordWrap: 'on',
      lineNumbers: 'on',
      folding: true,
      lineDecorationsWidth: 10,
      lineNumbersMinChars: 3,
      renderLineHighlight: 'line',
      selectOnLineNumbers: true,
      roundedSelection: false,
      cursorStyle: 'line',
      cursorBlinking: 'blink',
      cursorWidth: 0,
      renderWhitespace: 'selection',
      contextmenu: true,
      mouseWheelZoom: true,
      quickSuggestions: true,
      suggestOnTriggerCharacters: true,
      acceptSuggestionOnEnter: 'on',
      tabCompletion: 'on',
      wordBasedSuggestions: 'off',
      parameterHints: { enabled: true },
      hover: { enabled: true },
      formatOnPaste: true,
      formatOnType: true,
    })

    monacoEditorRef.current = editor
    setIsEditorReady(true)

    // Handle content changes
    editor.onDidChangeModelContent(() => {
      const newValue = editor.getValue()
      onChangeRef.current(newValue)
    })

    // Keydown handler removed after debugging

    // Handle validation changes
    if (onValidationChange) {
      const disposable = monaco.editor.onDidChangeMarkers((uris) => {
        const model = editor.getModel()
        if (model && uris.includes(model.uri)) {
          const markers = monaco.editor.getModelMarkers({ resource: model.uri })
          const isValid = markers.length === 0
          onValidationChange(isValid, markers)
        }
      })

      return () => {
        disposable.dispose()
        editor.dispose()
      }
    }

    return () => {
      editor.dispose()
    }
  }, [])

  // Update JSON diagnostics when schema changes
  useEffect(() => {
    monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
      validate: true,
      allowComments: false,
      schemas: schema
        ? [
            {
              uri: 'http://myserver/schema.json',
              fileMatch: ['*'],
              schema: schema,
            },
          ]
        : [],
    })
  }, [schema])

  // Update editor value when prop changes
  useEffect(() => {
    if (
      monacoEditorRef.current &&
      monacoEditorRef.current.getValue() !== value
    ) {
      monacoEditorRef.current.setValue(value || '')
    }
  }, [value])

  // Update readOnly option when prop changes
  useEffect(() => {
    if (monacoEditorRef.current) {
      monacoEditorRef.current.updateOptions({ readOnly })
    }
  }, [readOnly])

  // Register completion for userId values (Ctrl+Space and typing)
  useEffect(() => {
    if (!isEditorReady) return
    const disposable = monaco.languages.registerCompletionItemProvider('json', {
      triggerCharacters: [
        '"',
        '-',
        '_',
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'i',
        'j',
        'k',
        'l',
        'm',
        'n',
        'o',
        'p',
        'q',
        'r',
        's',
        't',
        'u',
        'v',
        'w',
        'x',
        'y',
        'z',
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z',
      ],
      provideCompletionItems: (model, position) => {
        try {
          // Inspect only current line up to caret for matching context
          // Only suggest when caret is in the value position of "userId": "..."
          // Check within the current line up to the caret for flexibility
          const lineContent = model.getLineContent(position.lineNumber)
          const uptoCol = lineContent.slice(0, Math.max(0, position.column - 1))
          const nearUserId = /"userId"\s*:\s*"?[^"\n]*$/.test(uptoCol)
          if (!nearUserId) return { suggestions: [] }

          const word = model.getWordUntilPosition(position)
          const range: monaco.IRange = {
            startLineNumber: position.lineNumber,
            endLineNumber: position.lineNumber,
            startColumn: word.startColumn,
            endColumn: word.endColumn,
          }

          const lastQuoteIdx = uptoCol.lastIndexOf('"')
          const prefix =
            lastQuoteIdx >= 0 ? uptoCol.slice(lastQuoteIdx + 1) : word.word

          const suggestions = (userIdOptions || [])
            .filter((u) =>
              prefix ? u.toLowerCase().startsWith(prefix.toLowerCase()) : true
            )
            .slice(0, 50)
            .map((u) => ({
              label: u,
              kind: monaco.languages.CompletionItemKind.Value,
              insertText: u,
              range,
            }))

          return { suggestions }
        } catch {
          return { suggestions: [] }
        }
      },
    })
    return () => disposable.dispose()
  }, [isEditorReady, userIdOptions])

  // Update language when prop changes
  useEffect(() => {
    if (monacoEditorRef.current && isEditorReady) {
      const model = monacoEditorRef.current.getModel()
      if (model) {
        monaco.editor.setModelLanguage(model, language)
      }
    }
  }, [language, isEditorReady])

  return (
    <div
      style={{
        height: height,
        border: '1px solid #d9d9d9',
        borderRadius: '6px',
      }}
    >
      <div ref={editorRef} style={{ height: '100%' }} />
    </div>
  )
}

export default MonacoEditor
