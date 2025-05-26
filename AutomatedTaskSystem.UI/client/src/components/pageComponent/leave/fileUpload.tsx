import { useState, useRef, ChangeEvent, DragEvent } from "react";
import { Upload, X, FileText, Image, File } from "lucide-react";

interface FileUploadProps {
  onFileChange: (file: File | null) => void;
  accept: string;
  maxSize?: number;
  label?: string;
  multiple?: boolean;
  onMultipleFileChange?: (files: File[]) => void;
  currentFiles?: File[];
}

const FileUpload = ({ 
  onFileChange, 
  accept, 
  maxSize = 10 * 1024 * 1024, 
  label = "Select file",
  multiple = false,
  onMultipleFileChange,
  currentFiles = []
}: FileUploadProps) => {
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string>("");
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragEnter = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const validateFile = (file: File): boolean => {
    const acceptedTypes = accept.split(',').map(type => type.trim());
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    
    if (!acceptedTypes.some(type => 
      type === fileExtension || 
      file.type.includes(type.replace('.', '')) ||
      (type.includes('pdf') && file.type.includes('pdf')) ||
      (type.includes('image') && file.type.startsWith('image/')) ||
      (type.includes('doc') && (file.type.includes('document') || file.type.includes('word')))
    )) {
      setError(`Please upload files of type: ${accept}`);
      return false;
    }
    
    if (file.size > maxSize) {
      const maxSizeMB = Math.round(maxSize / (1024 * 1024));
      setError(`File size should not exceed ${maxSizeMB}MB`);
      return false;
    }
    return true;
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
    setError("");

    const droppedFiles = Array.from(e.dataTransfer.files);
    
    if (multiple) {
      const validFiles = droppedFiles.filter(validateFile);
      if (validFiles.length > 0 && onMultipleFileChange) {
        onMultipleFileChange([...currentFiles, ...validFiles]);
      }
    } else {
      const droppedFile = droppedFiles[0];
      if (droppedFile && validateFile(droppedFile)) {
        setFile(droppedFile);
        onFileChange(droppedFile);
      }
    }
  };

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>): void => {
    setError("");
    const selectedFiles = e.target.files ? Array.from(e.target.files) : [];
    
    if (multiple) {
      const validFiles = selectedFiles.filter(validateFile);
      if (validFiles.length > 0 && onMultipleFileChange) {
        onMultipleFileChange([...currentFiles, ...validFiles]);
      }
    } else {
      const selectedFile = selectedFiles[0];
      if (selectedFile && validateFile(selectedFile)) {
        setFile(selectedFile);
        onFileChange(selectedFile);
      }
    }
    
    // Reset input value
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleReset = (): void => {
    setFile(null);
    setError("");
    onFileChange(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const removeFileFromMultiple = (indexToRemove: number): void => {
    if (onMultipleFileChange) {
      const updatedFiles = currentFiles.filter((_, index) => index !== indexToRemove);
      onMultipleFileChange(updatedFiles);
    }
  };

  const getFileIcon = (fileName: string) => {
    const extension = fileName.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'pdf':
        return <FileText className="h-5 w-5 text-red-500" />;
      case 'jpg':
      case 'jpeg':
      case 'png':
        return <Image className="h-5 w-5 text-blue-500" />;
      case 'doc':
      case 'docx':
        return <File className="h-5 w-5 text-blue-600" />;
      default:
        return <File className="h-5 w-5 text-gray-500" />;
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <div className="w-full">
      <div
        className={`relative border-2 border-dashed rounded-lg p-6 transition-all ${
          isDragging
            ? "border-blue-500 bg-blue-50"
            : "border-gray-300 hover:border-gray-400"
        } ${error ? "border-red-500 bg-red-50" : ""}`}
        onDragEnter={handleDragEnter}
        onDragOver={handleDragEnter}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        role="button"
        tabIndex={0}
        aria-label="File upload area"
      >
        <input
          type="file"
          ref={fileInputRef}
          className="hidden"
          accept={accept}
          multiple={multiple}
          onChange={handleFileChange}
          aria-label="File input"
        />

        <div className="text-center">
          {!multiple && !file ? (
            <>
              <Upload className="mx-auto h-10 w-10 text-gray-400" />
              <div className="mt-3">
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    fileInputRef.current?.click();
                  }}
                  className="px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded-md"
                >
                  {label}
                </button>
                <p className="mt-2 text-sm text-gray-500">
                  أو اسحب وأفلت الملف هنا
                </p>
              </div>
              <p className="mt-1 text-xs text-gray-500">
                {accept} up to {Math.round(maxSize / (1024 * 1024))}MB
              </p>
            </>
          ) : multiple ? (
            <>
              <Upload className="mx-auto h-10 w-10 text-gray-400" />
              <div className="mt-3">
                <button
                  onClick={(e) => {
                    e.preventDefault();
                    fileInputRef.current?.click();
                  }}
                  className="px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded-md"
                >
                  {label}
                </button>
                <p className="mt-2 text-sm text-gray-500">
                  أو اسحب وأفلت الملفات هنا
                </p>
              </div>
              <p className="mt-1 text-xs text-gray-500">
                {accept} up to {Math.round(maxSize / (1024 * 1024))}MB each
              </p>
            </>
          ) : (
            <div className="flex items-center justify-center space-x-4">
              {getFileIcon(file!.name)}
              <div className="flex-1 text-right">
                <span className="text-sm text-gray-700">{file!.name}</span>
                <p className="text-xs text-gray-500">{formatFileSize(file!.size)}</p>
              </div>
              <button
                onClick={handleReset}
                className="p-1 rounded-full hover:bg-gray-100"
                aria-label="Remove file"
              >
                <X className="h-5 w-5 text-gray-500" />
              </button>
            </div>
          )}
        </div>

        {error && (
          <div className="mt-3 p-3 bg-red-100 rounded-md">
            <p className="text-sm text-red-600">{error}</p>
          </div>
        )}
      </div>

      {/* Display multiple files */}
      {multiple && currentFiles.length > 0 && (
        <div className="mt-4 space-y-2">
          <p className="text-sm font-medium text-gray-700">الملفات المرفقة:</p>
          {currentFiles.map((file, index) => (
            <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div className="flex items-center space-x-3">
                {getFileIcon(file.name)}
                <div className="text-right">
                  <p className="text-sm text-gray-700 font-medium">{file.name}</p>
                  <p className="text-xs text-gray-500">{formatFileSize(file.size)}</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => removeFileFromMultiple(index)}
                className="p-1 rounded-full hover:bg-red-100 text-red-500 hover:text-red-700"
                aria-label="Remove file"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};


export default FileUpload;