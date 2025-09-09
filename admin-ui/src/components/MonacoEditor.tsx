import React, { useEffect, useRef, useState } from 'react'
import * as monaco from 'monaco-editor'

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
}

const MonacoEditor: React.FC<MonacoEditorProps> = ({
  value,
  onChange,
  language = 'json',
  height = 400,
  readOnly = false,
  schema,
  onValidationChange,
}) => {
  const editorRef = useRef<HTMLDivElement>(null)
  const monacoEditorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(
    null
  )
  const [isEditorReady, setIsEditorReady] = useState(false)

  useEffect(() => {
    if (!editorRef.current) return

    // Configure Monaco environment
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
      onChange(newValue)
    })

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
  }, [language, onChange, onValidationChange, readOnly, schema, value])

  // Update editor value when prop changes
  useEffect(() => {
    if (
      monacoEditorRef.current &&
      monacoEditorRef.current.getValue() !== value
    ) {
      monacoEditorRef.current.setValue(value || '')
    }
  }, [value])

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
